# Moving from MassTransit 8

ForkForward Cargo is MassTransit 8.5.12 under new package names, with usage telemetry off by default. The namespaces,
the types, their members and the rest of the behaviour are the same.

## One step

Replace each `MassTransit` package reference with its ForkForward Cargo package:

| Remove | Add |
|---|---|
| `MassTransit` | `ForkForward.Cargo` |
| `MassTransit.Abstractions` | `ForkForward.Cargo.Abstractions` |
| `MassTransit.RabbitMQ` | `ForkForward.Cargo.RabbitMQ` |
| `MassTransit.Azure.ServiceBus.Core` | `ForkForward.Cargo.AzureServiceBus` |
| `MassTransit.EntityFrameworkCore` | `ForkForward.Cargo.EntityFrameworkCore` |
| `MassTransit.TestFramework` | `ForkForward.Cargo.Testing` |

For example:

```
dotnet remove package MassTransit.RabbitMQ
dotnet add package ForkForward.Cargo.RabbitMQ --prerelease
```

Each package brings `ForkForward.Cargo` and `ForkForward.Cargo.Abstractions` with it, as the MassTransit packages did.
`--prerelease` is needed while only previews are published.

That is all. The code stays exactly as it is, `using MassTransit;` included.

## What stays as it was

- **Namespaces and types.** `MassTransit`, `IBus`, `IConsumer<T>`, `AddMassTransit` and every other namespace, type
  and member keep their names.
- **Assemblies.** The packages contain `MassTransit.dll`, `MassTransit.Abstractions.dll` and the other assemblies under
  their original names, signed with the original key.
- **Behaviour.** The code is MassTransit 8.5.12, so it behaves the same, except for usage telemetry (below).
- **Documentation.** These pages are MassTransit 8's, with the names of the published packages updated.

## What else changes

- **Usage telemetry is off.** MassTransit 8 reports
  [usage telemetry](../documentation/configuration/usage-telemetry.md) to a server of the original project by default.
  ForkForward Cargo collects nothing unless it is turned on, and then posts the report only to the address you set
  (`ReportAddress`); without one, the report stays in the log. Calls to `DisableUsageTelemetry` keep working and are no
  longer needed.
- **Both packages at once.** A project must not reference ForkForward Cargo packages and the original `MassTransit`
  packages at the same time, directly or through another package: both contain `MassTransit.dll`. Move a project, and
  the libraries it uses, as a whole.
- **Packages not published yet.** The other transports (Amazon SQS, ActiveMQ, SQL, Azure Functions), the riders (Kafka,
  Event Hub), the saga persistence other than Entity Framework Core, scheduling with Quartz.NET or Hangfire, and the
  remaining integrations are not published yet. A project that needs one of them cannot move until it is; see
  [Packages](../support/packages.md).
- **Version numbers** continue from 8.5.12. Previews are `8.6.0-preview.0.N`; stable releases will be published from
  version tags such as `v8.6.0`.
- **Samples and templates** on the MassTransit GitHub organization, and the `MassTransit.Templates` for `dotnet new`,
  belong to the original project and reference its packages. Replace those references before building on them.

## Moving from MassTransit 9

MassTransit 9 is published under a commercial licence and was never part of this fork; its new features are not here.
Code written for 9 may use API that 8 does not have. Everything described on these pages is available.
