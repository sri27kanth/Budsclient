﻿using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using GalaxyBudsClient.Cli;
using GalaxyBudsClient.Cli.Ipc;
using GalaxyBudsClient.Model.Config.Legacy;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Settings = GalaxyBudsClient.Model.Config.Settings;
#if !DEBUG
using Sentry;
#endif

namespace GalaxyBudsClient;

// Called by AsyncErrorHandler.Fody using IL weaving
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class AsyncErrorHandler
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static void HandleException(Exception exception)
    {
#if !DEBUG
        SentrySdk.CaptureException(exception);
#endif
        Log.Error(exception, "Unhandled exception in async task");
    }
}

public static class Program
{
    internal static long StartedAt;
    public static readonly string AvaresUrl = "avares://" + typeof(Program).Assembly.GetName().Name;

    public static void Startup(bool cliMode, ILogEventSink? additionalLogSink = null)
    {
        StartedAt = Stopwatch.GetTimestamp();
     
#if Windows
        GalaxyBudsClient.Platform.Windows.WindowsUtils.AttachConsole();
#endif

        var logPath = PlatformUtils.CombineDataPath("application.log");
        var prevLogPath = PlatformUtils.CombineDataPath("application-prev.log");
        // Rotate logs on startup
        try
        {
            if (File.Exists(logPath))
                File.Move(logPath, prevLogPath, true);
        }
        catch (Exception)
        {
            // Windows: exception is thrown when two instances are launched, because the first one is still using the log file
        }

        var config = new LoggerConfiguration()
            .WriteTo.File(logPath)
            .WriteTo.Console();

        if(additionalLogSink != null)
            config = config.WriteTo.Sink(additionalLogSink, LogEventLevel.Verbose);
        
        config = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VERBOSE")) ?
            config.MinimumLevel.Verbose() : config.MinimumLevel.Debug();
            
        // Divert program startup flow if the app was started with arguments (except /StartMinimized)
        if (cliMode)
        {
            // Disable excessive logging in CLI mode
            config = config.MinimumLevel.Warning();
        }
        
        if (!Settings.Data.DisableCrashReporting)
        {
            CrashReports.SetupCrashHandler();
            config = config.WriteTo.Sentry(o =>
            {
                o.InitializeSdk = false;
                o.MinimumBreadcrumbLevel = LogEventLevel.Debug;
                o.MinimumEventLevel = LogEventLevel.Fatal;
            });
        }
        
        if (!Directory.Exists(PlatformUtils.AppDataPath))
            Directory.CreateDirectory(PlatformUtils.AppDataPath);
        
        Log.Logger = config.CreateLogger();
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Trace.Listeners.Add(new ConsoleTraceListener());
        
        LegacySettings.BeginMigration();
    }
    
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    public static void Main(string[] args)
    {
        var cliMode = args.Length > 0 && !args.Contains("/StartMinimized");
        Startup(cliMode);
        
        if (cliMode)
        {
            CliHandler.ProcessArguments(args);
            return;
        }
        
        try
        {
            /* OSX: Graphics must be drawn on the main thread.
             * Awaiting this call would implicitly cause the next code to run as a async continuation task.
             *
             * In general: Don't await this call to shave off about 1000ms of startup time.
             * The IpcService will terminate the app in time if another instance is already running.
             */
            _ = Task.Run(IpcService.Setup);
                
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args, ShutdownMode.OnExplicitShutdown);
        }
        // ReSharper disable once RedundantCatchClause
#pragma warning disable CS0168 // Variable is declared but never used
        catch (Exception ex)
#pragma warning restore CS0168 // Variable is declared but never used
        {
#if DEBUG
            throw;
#else
            SentrySdk.CaptureException(ex);
            Log.Error(ex, "Unhandled exception in main thread");
#endif
        }
    } 

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .With(new MacOSPlatformOptions
            {
                // https://github.com/AvaloniaUI/Avalonia/issues/14577
                DisableSetProcessName = true
            })
            .UsePlatformDetect()
            .LogToTrace()
            .WithInterFont();
}