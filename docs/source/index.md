# ForkForward Cargo

The open-source continuation of MassTransit 8, the distributed application framework for .NET.

!!! info "The open-source continuation of MassTransit 8"

    ForkForward Cargo continues [MassTransit](https://github.com/MassTransit/MassTransit) 8 from the last code
    published under the Apache License 2.0, release 8.5.12. This documentation was written by Chris Patterson and the
    MassTransit contributors for MassTransit 8 and is inherited from the original project. It still describes this
    code: only the package names are new, and the namespaces, types and assemblies are still called `MassTransit`.
    The pages keep calling the framework MassTransit where they describe it; see
    [Moving from MassTransit 8](introduction/moving-from-masstransit.md) for what differs.

    ForkForward is an independent project. It is not affiliated with, sponsored by, or endorsed by Chris Patterson,
    Massient, Inc., or the MassTransit project. The official MassTransit is at [masstransit.io](https://masstransit.io).

Messages are the cargo: the framework carries them between services over a message broker such as RabbitMQ or Azure
Service Bus, and handles consumers, sagas, retries and the outbox on the way.

New to MassTransit? Start with the [Introduction](introduction/index.md) and the
[In Memory quick start](quick-starts/in-memory.md).

## Install

```
dotnet add package ForkForward.Cargo.RabbitMQ --prerelease
```

Only previews are published so far, so the commands on these pages use `--prerelease`.

| Package | Continues | Purpose |
|---|---|---|
| [ForkForward.Cargo](https://www.nuget.org/packages/ForkForward.Cargo) | `MassTransit` | Main package |
| [ForkForward.Cargo.Abstractions](https://www.nuget.org/packages/ForkForward.Cargo.Abstractions) | `MassTransit.Abstractions` | Abstractions |
| [ForkForward.Cargo.RabbitMQ](https://www.nuget.org/packages/ForkForward.Cargo.RabbitMQ) | `MassTransit.RabbitMQ` | RabbitMQ transport |
| [ForkForward.Cargo.AzureServiceBus](https://www.nuget.org/packages/ForkForward.Cargo.AzureServiceBus) | `MassTransit.Azure.ServiceBus.Core` | Azure Service Bus transport |
| [ForkForward.Cargo.EntityFrameworkCore](https://www.nuget.org/packages/ForkForward.Cargo.EntityFrameworkCore) | `MassTransit.EntityFrameworkCore` | Entity Framework Core integration |
| [ForkForward.Cargo.Testing](https://www.nuget.org/packages/ForkForward.Cargo.Testing) | `MassTransit.TestFramework` | Testing tools |

The other transports, the persistence and scheduling packages and the riders are not published yet: see
[Packages](support/packages.md).

## Housekeeping

The releases are on [NuGet](https://www.nuget.org/packages/ForkForward.Cargo), and every version tag has a
[GitHub release](https://github.com/ForkForward/dotnet-cargo/releases).

Questions and bug reports go to the [issues](https://github.com/ForkForward/dotnet-cargo/issues) of this fork, not to
the MassTransit project or to Massient: they are not responsible for this code. See [Getting help](support/index.md).
