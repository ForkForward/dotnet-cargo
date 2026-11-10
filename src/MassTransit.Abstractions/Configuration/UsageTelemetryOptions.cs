namespace MassTransit;

using System;


public class UsageTelemetryOptions
{
    /// <summary>
    /// If true, usage telemetry is collected and reported to the original MassTransit project
    /// (usage-tracking.masstransit.io). Defaults to false in ForkForward Cargo; MassTransit 8 defaulted to true.
    /// The environment variable MASSTRANSIT_USAGE_TELEMETRY, when set, takes precedence over this value.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Specify your CustomerId if you're an existing customer. If not specified, may be pulled from the license key if present.
    /// </summary>
    public string? CustomerId { get; set; }

    /// <summary>
    /// The delay after the bus has started before reporting usage telemetry
    /// </summary>
    public TimeSpan? ReportDelay { get; set; }

    /// <summary>
    /// If true, usage telemetry will be reported when the application is stopped (if it hasn't already been reported)
    /// </summary>
    public bool ReportOnShutdown { get; set; }

    /// <summary>
    /// If true, consumer, saga, and activity details are included in the usage telemetry. Defaults to false.
    /// </summary>
    public bool Verbose { get; set; }
}
