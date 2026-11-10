#nullable enable
namespace MassTransit.UsageTracking;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Configuration;
using Internals;
using Logging;
using Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using UsageTelemetry;


public class UsageTracker :
    IUsageTracker,
    IAsyncDisposable
{
    const string EnabledVariable = "MASSTRANSIT_USAGE_TELEMETRY";
    const string AddressVariable = "MASSTRANSIT_USAGE_TELEMETRY_ADDRESS";

    readonly TimeSpan _httpClientTimeout = TimeSpan.FromSeconds(10);
    readonly object _lock = new object();
    readonly UsageTelemetryOptions _options;
    readonly List<IUsageTelemetrySource> _usages;
    CancellationTokenSource? _cancellationTokenSource;
    bool _disposed;
    ILogContext? _logContext;
    Task? _reportTask;

    // Where the report is posted. There is no default address: without one, the report is only written to the log.
    readonly Uri? _reportAddress;
    readonly string? _invalidReportAddress;
    bool _reportAddressChecked;

    public UsageTracker(IOptions<UsageTelemetryOptions> usageTrackingOptions)
    {
        _options = usageTrackingOptions.Value;
        _usages = [];

        var variable = Environment.GetEnvironmentVariable(EnabledVariable);
        if (!string.IsNullOrWhiteSpace(variable))
        {
            if (bool.TryParse(variable, out var enabled))
                _options.Enabled = enabled;
            else if (int.TryParse(variable, out var value))
                _options.Enabled = value == 1;
            else
                LogContext.Warning?.Log("Unrecognized MASSTRANSIT_USAGE_TELEMETRY value: {Variable}. Should be true or false, 1 or 0", variable);
        }

        if (Debugger.IsAttached)
            _options.Enabled = false;

        if (_options.Enabled)
        {
            _reportAddress = GetReportAddress(_options, out _invalidReportAddress);

            Telemetry = new MassTransitUsageTelemetry
            {
                Id = NewId.NextGuid(),
                CustomerId = _options.CustomerId,
                Created = DateTimeOffset.Now.ToString("O"),
                Host = new HostUsageTelemetry
                {
                    MassTransitVersion = HostMetadataCache.Host.MassTransitVersion,
                    FrameworkVersion = HostMetadataCache.Host.FrameworkVersion,
                    OperatingSystemVersion = HostMetadataCache.Host.OperatingSystemVersion,
                    TimeZoneInfo = TimeZoneInfo.Local.ToString(),
                    CommitHash = HostMetadataCache.GetCommitHash()
                }
            };
        }
    }

    [MemberNotNullWhen(true, nameof(Enabled))]
    MassTransitUsageTelemetry? Telemetry { get; }

    public bool Enabled => _options.Enabled;

    public async ValueTask DisposeAsync()
    {
        _disposed = true;

        if (_reportTask is { IsCompleted: true })
            return;

        var cancellationTokenSource = _cancellationTokenSource;
        if (cancellationTokenSource == null)
            return;

        if (!cancellationTokenSource.IsCancellationRequested)
            cancellationTokenSource.Cancel();
        try
        {
            if (_reportTask != null)
                await _reportTask.OrTimeout(_httpClientTimeout).ConfigureAwait(false);
        }
        catch (Exception)
        {
            //
        }
    }

    public void PreConfigureBus<T>(T configurator, IBusRegistrationContext context)
        where T : IBusFactoryConfigurator
    {
        if (!_options.Enabled || Telemetry is null)
            return;

        lock (_lock)
        {
            _logContext ??= LogContext.Current;

            WarnIfNotReported();

            var busName = nameof(IBus);

            var selector = context.GetService<IContainerSelector>();
            if (selector != null && selector.GetType().ClosesType(typeof(DependencyInjectionContainerRegistrar<>), out Type[] types))
                busName = types[0].Name;

            var busUsageTelemetry = new BusUsageTelemetry
            {
                Name = busName,
                Created = DateTimeOffset.Now.ToString("O"),
                Endpoints = []
            };
            Telemetry.Bus ??= [];
            Telemetry.Bus.Add(busUsageTelemetry);

            var observer = new UsageTelemetryBusObserver(this, busUsageTelemetry);
            configurator.ConnectBusObserver(observer);

            var endpointConfigurationObserver = new UsageTelemetryEndpointConfigurationObserver(busUsageTelemetry, Telemetry);

            _ = configurator.ConnectEndpointConfigurationObserver(endpointConfigurationObserver);

            _usages.Add(endpointConfigurationObserver);
        }
    }

    public void PreConfigureRider<T>(T configurator)
        where T : IRiderFactoryConfigurator
    {
        if (!_options.Enabled || Telemetry is null)
            return;

        lock (_lock)
        {
            _logContext ??= LogContext.Current;

            var riderType = configurator.GetType().Name switch
            {
                "EventHubFactoryConfigurator" => "EventHub",
                "KafkaFactoryConfigurator" => "Kafka",
                _ => configurator.GetType().Name
            };

            Telemetry.Rider ??= [];
            var riderUsage = Telemetry.Rider.LastOrDefault(x => x.RiderType == riderType);
            if (riderUsage == null)
            {
                riderUsage = new RiderUsageTelemetry
                {
                    RiderType = riderType,
                    Endpoints = []
                };
                Telemetry.Rider.Add(riderUsage);
            }
        }
    }

    public void PostCreateBus(IBus bus, BusUsageTelemetry busTelemetry)
    {
        if (!_options.Enabled)
            return;

        _logContext ??= LogContext.Current;

        busTelemetry.Configured = DateTimeOffset.Now.ToString("O");
    }

    public void PostStartBus(IBus bus, BusUsageTelemetry busTelemetry)
    {
        if (!_options.Enabled || Telemetry is null)
            return;

        lock (_lock)
        {
            _logContext ??= LogContext.Current;

            busTelemetry.Started = DateTimeOffset.Now.ToString("O");

            if (Telemetry.Bus?.Any(x => x.Started is null) ?? true)
                return;

            _usages.ForEach(x => x.Update());

            if (_disposed)
                return;

            _reportTask ??= Task.Run(() => ReportUsageTelemetry());
        }
    }

    async Task ReportUsageTelemetry()
    {
        if (Telemetry is null)
            return;

        if (_disposed)
            return;

        if (_logContext is not null)
            LogContext.SetCurrentIfNull(_logContext);

        try
        {
            Interlocked.CompareExchange(ref _cancellationTokenSource, new CancellationTokenSource(), null);

            var delay = _options.ReportDelay ?? TimeSpan.FromMinutes(5);

            var maxDelay = TimeSpan.FromMinutes(30);
            if (delay > maxDelay)
                delay = maxDelay;
            if (delay < TimeSpan.FromSeconds(30))
                delay = TimeSpan.FromSeconds(30);

            try
            {
                await Task.Delay(delay, _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
            }

            if (_cancellationTokenSource.IsCancellationRequested && _options.ReportOnShutdown == false)
                return;

            CloudEnvironmentInfo? cloudEnvironment = await DetectCloudEnvironment();

            string json;
            lock (_lock)
            {
                if (cloudEnvironment.HasValue)
                {
                    Telemetry!.Host!.Cloud = cloudEnvironment.Value.Provider;
                    Telemetry!.Host!.Region = cloudEnvironment.Value.Region;
                }

                if (HostMetadataCache.IsRunningInContainer)
                    Telemetry!.Host!.Container = HostMetadataCache.IsKubernetes ? "Kubernetes" : "Docker";

                json = JsonSerializer.Serialize(Telemetry!, UsageTelemetrySerializerContext.Default.MassTransitUsageTelemetry);
            }

            LogContext.Info?.Log("Usage Telemetry: {Telemetry}", json);

            // Without a valid address the report stays in the log; the warning was written when the bus was configured.
            if (_reportAddress is null)
                return;

            using var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");

            using var httpClient = new HttpClient();
            httpClient.Timeout = _httpClientTimeout;

            try
            {
                using var response = await httpClient.PostAsync(_reportAddress, jsonContent);
                if (!response.IsSuccessStatusCode)
                    LogContext.Warning?.Log("Failed to report usage telemetry to {Address}: {StatusCode}", _reportAddress, response.StatusCode);
            }
            catch (Exception exception)
            {
                // Includes the timeout, which HttpClient reports as a cancellation and the catch below would hide.
                LogContext.Warning?.Log(exception, "Failed to report usage telemetry to {Address}", _reportAddress);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            LogContext.Warning?.Log(exception, "Failed to report usage telemetry");
        }
    }

    /// <summary>
    /// The environment variable takes precedence over the options. Only an absolute http or https address is used.
    /// </summary>
    static Uri? GetReportAddress(UsageTelemetryOptions options, out string? invalidAddress)
    {
        invalidAddress = null;

        var variable = Environment.GetEnvironmentVariable(AddressVariable);
        var configured = string.IsNullOrWhiteSpace(variable) ? options.ReportAddress?.OriginalString : variable!.Trim();
        if (string.IsNullOrWhiteSpace(configured))
            return null;

        if (Uri.TryCreate(configured, UriKind.Absolute, out var address)
            && (address.Scheme == Uri.UriSchemeHttp || address.Scheme == Uri.UriSchemeHttps))
            return address;

        invalidAddress = configured;
        return null;
    }

    /// <summary>
    /// Says once, when the first bus is configured, that the report will not be sent anywhere.
    /// </summary>
    void WarnIfNotReported()
    {
        if (_reportAddressChecked)
            return;

        _reportAddressChecked = true;

        if (_reportAddress is not null)
            return;

        if (_invalidReportAddress is null)
        {
            LogContext.Warning?.Log("Usage telemetry is enabled, but no report address is set (UsageTelemetryOptions.ReportAddress or {Variable})."
                + " The report is written to the log only and is not sent.", AddressVariable);
        }
        else
        {
            LogContext.Warning?.Log("Usage telemetry is enabled, but the report address {Address} is not an absolute http or https address."
                + " The report is written to the log only and is not sent.", _invalidReportAddress);
        }
    }

    static async Task<CloudEnvironmentInfo?> DetectCloudEnvironment()
    {
        var region = Environment.GetEnvironmentVariable("WEBSITE_REGION") ?? Environment.GetEnvironmentVariable("REGION_NAME");
        if (!string.IsNullOrEmpty(region))
            return new CloudEnvironmentInfo("Azure", region);

        region = Environment.GetEnvironmentVariable("AWS_REGION") ?? Environment.GetEnvironmentVariable("AWS_DEFAULT_REGION");
        if (!string.IsNullOrEmpty(region))
            return new CloudEnvironmentInfo("AWS", region);

        var gcpProject = Environment.GetEnvironmentVariable("GOOGLE_CLOUD_PROJECT");
        if (!string.IsNullOrEmpty(gcpProject))
        {
            try
            {
                var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };

                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("Metadata-Flavor", "Google");
                var zone = await httpClient.GetStringAsync("http://metadata.google.internal/computeMetadata/v1/instance/zone");
                var match = Regex.Match(zone, @"zones/([a-z\-]+)");
                if (match.Success)
                {
                    region = match.Groups[1].Value.Replace("-[a-z]$", "");
                    return new CloudEnvironmentInfo("GCP", region);
                }
            }
            catch
            {
                //
            }

            return new CloudEnvironmentInfo("GCP", "Unknown");
        }

        return null;
    }


    readonly struct CloudEnvironmentInfo
    {
        public readonly string Provider;
        public readonly string Region;

        public CloudEnvironmentInfo(string provider, string region)
        {
            Provider = provider;
            Region = region;
        }
    }
}
