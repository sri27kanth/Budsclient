using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model.Config;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog;

namespace GalaxyBudsClient.Scripting.Experiment;

public class ExperimentClient
{
    public event EventHandler<IReadOnlyList<ExperimentRequest>>? NewResultsFound;

    private const string ApiBase = "https://crowdsourcing.timschneeberger.me/v2";
    private const string ApiGetExperiments = ApiBase + "/experiments";
    private const string ApiPostResult = ApiBase + "/result";

    private readonly HttpClient _client;
    private readonly Timer _timer;
    public ExperimentClient()
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = delegate { return true; }
        };
        _client = new HttpClient(handler);
        _timer = new Timer(120 * 60 * 1000)
        {
            AutoReset = true
        };
        _timer.Elapsed += (sender, args) => ScanForExperiments();
        _timer.Start();
    }

    public async Task<bool> PostResult(ExperimentResult result)
    {
        Log.Debug("ExperimentClient: Posting results for experiment #{Id}...", result.ExperimentId);
        try
        {
            var jsonBody = JsonConvert.SerializeObject(result, new StringEnumConverter());
            var httpContent = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            var httpResponse = await _client.PostAsync(ApiPostResult, httpContent);
            if (!httpResponse.IsSuccessStatusCode)
            {
                Log.Warning("ExperimentClient: Server returned error code after posting: {Code} ({ReasonPhrase}); Content: {Content}", 
                    (int)httpResponse.StatusCode, httpResponse.ReasonPhrase, await httpResponse.Content.ReadAsStringAsync());
            }
            else
            {
                return true;
            }
        }
        catch (HttpRequestException ex)
        {
            Log.Error("ExperimentClient: Post failed due to network issues: {Message}", ex.Message);
        }
        catch (Exception ex)
        {
            Log.Error("ExperimentClient: Post failed: {Message}", ex.Message);
        }

        return false;
    }

    public async void ScanForExperiments()
    {
        if (!BluetoothImpl.Instance.IsConnected || 
            BluetoothImpl.Instance.CurrentModel == Models.NULL ||
            DeviceMessageCache.Instance.ExtendedStatusUpdate == null)
        {
            return;
        }
            
        // Android builds are AOT compiled and do not support cs-script
        if (Settings.Data.ExperimentsDisabled || !PlatformUtils.IsDesktop)
        {
            Log.Information("ExperimentClient: Feature is disabled");
            return;
        }

        ExperimentRequest[]? requests = null;
        try
        {
            var response =
                await _client.GetAsync($"{ApiGetExperiments}/{BluetoothImpl.Instance.CurrentModel.ToString()}");
            if (response.IsSuccessStatusCode)
            {
                var formatters = new MediaTypeFormatterCollection();
                formatters.JsonFormatter.SerializerSettings.Converters.Add(new StringEnumConverter());
                    
                requests = await response.Content.ReadAsAsync<ExperimentRequest[]>(formatters);
            }
            else
            {
                Log.Error("ExperimentClient: HTTP error {Code}", response.StatusCode);
            }

            if (requests == null)
            {
                Log.Warning("ExperimentClient: Scan failed; no data received");
                return;
            }

            var results = requests
                .VerifyDecode()
                .Where(ExperimentRequestFilters.FilterByEnvironment)
                .Where(ExperimentRequestFilters.FilterByVersion)
                .Where(ExperimentRequestFilters.IsNotDone)
                .ToList()
                .AsReadOnly();

            NewResultsFound?.Invoke(this, results);
        }
        catch (HttpRequestException ex)
        {
            Log.Error("ExperimentClient: Scan failed due to network issues: {Message}", ex.Message);
        }
        catch (Exception ex)
        {
            Log.Error("ExperimentClient: Scan failed: {Message}", ex.Message);
        }
    }
}