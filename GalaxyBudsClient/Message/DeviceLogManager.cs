using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Message.Encoder;
using GalaxyBudsClient.Platform;
using Serilog;
using Task = System.Threading.Tasks.Task;

namespace GalaxyBudsClient.Message;

public class LogDownloadProgressEventArgs(
    int currentByteCount,
    int totalByteCount,
    LogDownloadProgressEventArgs.Type type)
    : EventArgs
{
    public enum Type
    {
        Trace,
        Coredump,
        _Switching
    }

    public int CurrentByteCount { get; } = currentByteCount;
    public int TotalByteCount { get; } = totalByteCount;
    public Type DownloadType { get;  } = type;
}
    
public class LogDownloadFinishedEventArgs(List<string> traceDumpPaths, List<string> coreDumpPaths) : EventArgs
{
    public List<string> TraceDumpPaths { get; } = traceDumpPaths;
    public List<string> CoreDumpPaths { get; } = coreDumpPaths;
}
    
public class DeviceLogManager
{
    public event EventHandler<LogDownloadProgressEventArgs>? ProgressUpdated;
    public event EventHandler<LogDownloadFinishedEventArgs>? Finished;

    private bool _hasCompletedRoleSwitch = false;
        
    private byte[]? _traceBuffer;
    private byte[]? _coredumpBuffer;
    private string _startTimestamp = string.Empty;
        
    private readonly Dictionary<int, int> _offsetList = new();
    private LogTraceStartParser? _traceContext;
    private LogCoredumpDataSizeParser? _coredumpContext;
        
    private readonly List<string> _coreDumpPaths = [];
    private readonly List<string> _traceDumpPaths = [];

    public async Task BeginDownloadAsync()
    {
        _coreDumpPaths.Clear();
        _traceDumpPaths.Clear();
        _hasCompletedRoleSwitch = false;
        _startTimestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        BluetoothService.Instance.MessageReceived += OnMessageReceived;
        await BluetoothService.Instance.SendRequestAsync(SppMessage.MessageIds.LOG_SESSION_OPEN);
    }

    public async Task CancelDownload()
    {
        await BluetoothService.Instance.SendRequestAsync(SppMessage.MessageIds.LOG_SESSION_CLOSE);
        BluetoothService.Instance.MessageReceived -= OnMessageReceived;
            
        await BluetoothService.Instance.DisconnectAsync();
    }
        
    private async Task FinishDownload()
    {
        Finished?.Invoke(this, new LogDownloadFinishedEventArgs(_traceDumpPaths, _coreDumpPaths));
        await CancelDownload();
    }

    private static string? WriteTempFile(string filename, byte[] content)
    {
        try
        {
            var path = Path.Combine(Path.GetTempPath(), filename);
            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            fs.Write(content, 0, content.Length);
            return path;
        }
        catch (Exception ex)
        {
            Log.Error("DeviceLogManager.WriteTempFile: Cannot write '{File}': {Message}", 
                filename, ex.Message);
        }

        return null;
    }
        
    private async void OnMessageReceived(object? sender, SppMessage e)
    {
        switch (e.Id)
        {
            #region SESSION
            case SppMessage.MessageIds.LOG_SESSION_OPEN:
                _hasCompletedRoleSwitch = false;
                if (e.Payload[0] == 0)
                {
                    await BluetoothService.Instance.SendRequestAsync(SppMessage.MessageIds.LOG_TRACE_START, 0);
                }
                break;
            case SppMessage.MessageIds.LOG_SESSION_CLOSE:
                break;
            #endregion
                
            #region TRACE
            case SppMessage.MessageIds.LOG_TRACE_START:
                _traceContext = e.BuildParser() as LogTraceStartParser;

                _traceBuffer = new byte[_traceContext!.DataSize];
                MakeOffsetList(_traceContext.FragmentCount, _traceContext.PartialDataMaxSize);
                    
                await BluetoothService.Instance.SendAsync(LogTraceDataEncoder.Build(0, _traceContext.DataSize));
                break;
            case SppMessage.MessageIds.LOG_TRACE_DATA:
                var data = e.BuildParser() as LogTraceDataParser;
                UpdateOffsetList(data!.PartialDataOffset);
                    
                Array.Copy(data.RawData, 0, _traceBuffer!, data.PartialDataOffset, data.PartialDataSize);
                    
                ProgressUpdated?.Invoke(this, new LogDownloadProgressEventArgs(data.PartialDataOffset, _traceContext!.DataSize, LogDownloadProgressEventArgs.Type.Trace));
                break;
            case SppMessage.MessageIds.LOG_TRACE_ROLE_SWITCH:
                var result = e.Payload[0] == 0;
                _hasCompletedRoleSwitch = true;

                if (result)
                {
                    await BluetoothService.Instance.SendRequestAsync(SppMessage.MessageIds.LOG_TRACE_START, 0);
                }
                else
                {
                    await BluetoothService.Instance.SendRequestAsync(SppMessage.MessageIds.LOG_SESSION_CLOSE);
                }
                break;
            case SppMessage.MessageIds.LOG_TRACE_DATA_DONE:
                var remainOffset = GetRemainOffset();
                if (remainOffset >= 0)
                {
                    int i = _traceContext!.PartialDataMaxSize;
                    var i2 = remainOffset + i;
                    var i3 = _traceContext!.DataSize;
                    if (i2 > i3)
                    {
                        i = i3 - remainOffset;
                    }

                    await BluetoothService.Instance.SendAsync(LogTraceDataEncoder.Build(remainOffset, i));
                    return;
                }

                ProgressUpdated?.Invoke(this, new LogDownloadProgressEventArgs(0,0, LogDownloadProgressEventArgs.Type._Switching));
                await BluetoothService.Instance.SendRequestAsync(SppMessage.MessageIds.LOG_TRACE_COMPLETE);

                var path = WriteTempFile($"{BluetoothService.ActiveModel.ToString()}_traceDump_{_traceContext?.DeviceType.ToString()}_{_startTimestamp}.bin", _traceBuffer ?? Array.Empty<byte>());
                if (path != null)
                {
                    _traceDumpPaths.Add(path);
                }
                break;
            case SppMessage.MessageIds.LOG_TRACE_COMPLETE:
                await BluetoothService.Instance.SendRequestAsync(SppMessage.MessageIds.LOG_COREDUMP_DATA_SIZE);
                break;
            #endregion

            #region COREDUMP
            case SppMessage.MessageIds.LOG_COREDUMP_DATA_SIZE:
                _coredumpContext = e.BuildParser() as LogCoredumpDataSizeParser;
                MakeOffsetList(_coredumpContext!.FragmentCount, _coredumpContext.PartialDataMaxSize);

                if (_coredumpContext.DataSize > 0)
                {
                    _coredumpBuffer = new byte[_coredumpContext.DataSize];
                    await BluetoothService.Instance.SendAsync(LogCoredumpDataEncoder.Build(0, _coredumpContext.DataSize));
                }
                else if (_hasCompletedRoleSwitch) 
                {
                    await FinishDownload();
                } 
                else
                {
                    await BluetoothService.Instance.SendRequestAsync(SppMessage.MessageIds.LOG_TRACE_ROLE_SWITCH);
                }
                break;
            case SppMessage.MessageIds.LOG_COREDUMP_DATA:
                var coredumpData = e.BuildParser() as LogCoredumpDataParser;
                UpdateOffsetList(coredumpData!.PartialDataOffset);
                    
                Array.Copy(coredumpData.RawData, 0, _coredumpBuffer!, coredumpData.PartialDataOffset, coredumpData.PartialDataSize);
                    
                ProgressUpdated?.Invoke(this, new LogDownloadProgressEventArgs(coredumpData.PartialDataOffset, _traceContext!.DataSize, LogDownloadProgressEventArgs.Type.Coredump));
                break;
            case SppMessage.MessageIds.LOG_COREDUMP_DATA_DONE:
                var remainOffsetCore = GetRemainOffset();
                if (remainOffsetCore >= 0)
                {
                    int i = _traceContext!.PartialDataMaxSize;
                    var i2 = remainOffsetCore + i;
                    var i3 = _traceContext!.DataSize;
                    if (i2 > i3)
                    {
                        i = i3 - remainOffsetCore;
                    }

                    await BluetoothService.Instance.SendAsync(LogCoredumpDataEncoder.Build(remainOffsetCore, i));
                    return;
                }
                    
                ProgressUpdated?.Invoke(this, new LogDownloadProgressEventArgs(0,0, LogDownloadProgressEventArgs.Type._Switching));
                await BluetoothService.Instance.SendRequestAsync(SppMessage.MessageIds.LOG_COREDUMP_COMPLETE);
                    
                var pathCore = WriteTempFile($"{BluetoothService.ActiveModel.ToString()}_coreDump_{/* this is intentional -> */_traceContext?.DeviceType.ToString()}_{_startTimestamp}.bin", _coredumpBuffer ?? Array.Empty<byte>());
                if (pathCore != null)
                {
                    _coreDumpPaths.Add(pathCore);
                }
                break;
            case SppMessage.MessageIds.LOG_COREDUMP_COMPLETE:
                if (_hasCompletedRoleSwitch)
                {
                    /* Everything is done */
                    await FinishDownload();
                }
                else
                {
                    await BluetoothService.Instance.SendRequestAsync(SppMessage.MessageIds.LOG_TRACE_ROLE_SWITCH);
                }
                break;
            #endregion

        }
    }
        
    private void MakeOffsetList(int fragmentCount, int maxDataSize) {
        _offsetList.Clear();
        for (var cnt = 0; cnt < fragmentCount; cnt++) {
            _offsetList.Add(cnt, cnt * maxDataSize);
        }
    }
        
    private void UpdateOffsetList(int i)
    {
        if (_offsetList.Count <= 0) 
            return;
            
        foreach (var pair in _offsetList.Where(pair => pair.Value == i))
        {
            _offsetList.Remove(pair.Key);
        }
    }

    private int GetRemainOffset() {
        if (_offsetList.Count <= 0)
            return -1;
            
        using var it = _offsetList.GetEnumerator();
        if (it.MoveNext())
        {
            return it.Current.Value;
        }
        return -1;
    }

    #region Singleton
    private static readonly object Padlock = new();
    private static DeviceLogManager? _instance;
    public static DeviceLogManager Instance
    {
        get
        {
            lock (Padlock)
            {
                return _instance ??= new DeviceLogManager();
            }
        }
    }

    public static void Init()
    {
        lock (Padlock)
        { 
            _instance ??= new DeviceLogManager();
        }
    }
    #endregion
}