# Usage Telemetry

!!! info "In ForkForward Cargo: off by default, and sent only where you say"

    MassTransit 8 collects this usage telemetry by default and sends it to
    `https://usage-tracking.masstransit.io/usage`, a server of the original MassTransit project. ForkForward Cargo
    changes two things:

    - It is **off by default**: nothing is collected or sent unless you turn it on.
    - It has **no built-in destination**: the report is posted only to the address you set. Without one, it is
      written to the log only, and a warning says so when the bus is configured.

    ```csharp
    services.AddMassTransit(x =>
    {
        x.ConfigureUsageTelemetryOptions(options =>
        {
            options.Enabled = true;
            options.ReportAddress = new Uri("https://telemetry.example.com/masstransit/usage");
        });
    });
    ```

    | Setting | Option | Environment variable |
    |---|---|---|
    | Turn it on | `Enabled = true` | `MASSTRANSIT_USAGE_TELEMETRY=true` |
    | Where to post it | `ReportAddress`, an absolute http or https address | `MASSTRANSIT_USAGE_TELEMETRY_ADDRESS` |

    An environment variable that is set takes precedence over the code, `DisableUsageTelemetry` included.

    **The report.** About five minutes after all buses have started, the application posts one JSON document to the
    address: the example under [Content](#content), with `Content-Type: application/json` and property names in
    snake_case. Any 2xx response counts as received. Anything else, a refused connection, or no answer within 10
    seconds is logged as a warning, and the application carries on. The same JSON is always written to the log at
    Information level (`Usage Telemetry: {...}`), so it also reaches whatever collects your logs. The receiver has
    to accept a plain JSON POST; an OpenTelemetry (OTLP) endpoint does not, because OTLP has its own format.

    **To report to the original MassTransit project**, as MassTransit 8 did, set the address to
    `https://usage-tracking.masstransit.io/usage`. ForkForward does not receive that data and has no say over what
    happens to it.

    The rest of this page is the original project's description, where "we" is the MassTransit team, written for when
    the telemetry was on by default and always sent to them.

As part of our ongoing commitment to improving MassTransit, in v8.40 we are introducing anonymous usage telemetry to better understand how the community utilizes the framework. This feature will provide valuable insights into bus configurations, transport usage, and endpoint patterns, helping us identify trends, optimize performance, and prioritize future enhancements. By collecting only non-identifiable data, we ensure user privacy while enabling more informed development decisions that benefit the entire MassTransit ecosystem.

We understand that not everyone may want to participate in usage telemetry, which is why we have made it easy to opt out. If you prefer not to send anonymous usage data, you can disable telemetry through a simple configuration setting. This ensures that no information is transmitted while still allowing you to use MassTransit without any impact on functionality. Our goal is to provide transparency and choice, so you can decide what works best for your environment.

!!! info

    The collected usage telemetry is strictly for internal use and is not shared, sold, or made publicly available. Its sole purpose is to help the MassTransit team analyze usage patterns, improve performance, and guide future development based on real-world adoption. No personally identifiable information is collected, ensuring that all data remains anonymous and focused solely on enhancing the framework for the community.

## Content

The example report below demonstrates the collected data. By default, only minimal details about the host, bus, and endpoints are included, along with basic configuration and counts for each consumer or message type.

```json
{
  "id": "bd740008-ebb8-e450-a00a-08dd60e7bc9c",
  "bus": [
    {
      "name": "IBus",
      "created": "2025-03-11T16:57:37.4587070-05:00",
      "started": "2025-03-11T16:57:37.9216500-05:00",
      "endpoints": [
        {
          "name": "odd-job",
          "type": "RabbitMQ",
          "consumer_count": 5,
          "prefetch_count": 64,
          "job_consumer_count": 1
        },
        {
          "name": "odd-job-completed",
          "type": "RabbitMQ",
          "consumer_count": 1,
          "prefetch_count": 1,
          "concurrent_message_limit": 1
        },
        {
          "name": "job-type",
          "type": "RabbitMQ",
          "prefetch_count": 64,
          "concurrent_message_limit": 16,
          "saga_state_machine_count": 1
        },
        {
          "name": "job",
          "type": "RabbitMQ",
          "prefetch_count": 64,
          "concurrent_message_limit": 16,
          "saga_state_machine_count": 1
        },
        {
          "name": "job-attempt",
          "type": "RabbitMQ",
          "prefetch_count": 64,
          "concurrent_message_limit": 16,
          "saga_state_machine_count": 1
        }
      ],
      "configured": "2025-03-11T16:57:37.8701060-05:00"
    }
  ],
  "host": {
     "commit_hash": "c0977e09482f7f5e3585c9a59e67941a89fd2963",
     "time_zone_info": "(UTC-06:00) Central Time (Chicago)",
    "framework_version": "9.0.0",
    "mass_transit_version": "1.0.0.0",
    "operating_system_version": "Unix 14.7.2"
  },
  "created": "2025-03-12T16:57:37.4572470-05:00"
}
```

## Reporting

Usage telemetry is sent once, five minutes after the bus starts (or after all buses have started if you're using MultiBus). If the bus runs for less than five minutes, no data is reported. Telemetry is not captured or sent when using the test harness or when debugging the application. Reporting is non-blocking and does not impact the startup or shutdown of MassTransit or your application. In ForkForward Cargo, reports are sent only to the address you set, as described above.

## Opt-Out

If you prefer not to send anonymous usage data, you can disable telemetry through a simple configuration setting. This ensures that no information is transmitted while still allowing you to use MassTransit without any impact on functionality.

#### Configuration

To disable usage telemetry, specify `DisableUsageTelemetry` on the MassTransit configuration. When disabled, no usage telemetry is captured or reported.

```csharp
services.AddMassTransit(x =>
{
    x.DisableUsageTelemetry();
});
```

#### Environment Variable

You can also disable usage telemetry using an environment variable. Set:

`MASSTRANSIT_USAGE_TELEMETRY=false`

To disable usage telemetry.

## Configuration

To provide additional information, such as your _CustomerId_, usage telemetry can be configured as shown.

```csharp
services.AddMassTransit(x =>
{
    x.ConfigureUsageTelemetryOptions(options =>
    {
        options.CustomerId = "8675309";
    });
});
```
