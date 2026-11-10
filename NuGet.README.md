# ForkForward Cargo

The open-source continuation of [MassTransit](https://github.com/MassTransit/MassTransit) 8, the distributed application framework for .NET created by Chris Patterson and Dru Sellers. It continues the last Apache 2.0-licensed code (8.5.12) under the Apache License 2.0. Independent project: not affiliated with, sponsored by, or endorsed by the original authors or Massient, Inc. The official MassTransit is at [masstransit.io](https://masstransit.io).

> **Preview.** These packages carry the code of MassTransit 8.5.12. The assemblies and namespaces are still called `MassTransit`, and they are signed with the original key, so a project must not reference these packages and the original `MassTransit` packages at the same time.

## Packages

| Package | Continues |
|---|---|
| `ForkForward.Cargo` | `MassTransit` |
| `ForkForward.Cargo.Abstractions` | `MassTransit.Abstractions` |
| `ForkForward.Cargo.RabbitMQ` | `MassTransit.RabbitMQ` |
| `ForkForward.Cargo.AzureServiceBus` | `MassTransit.Azure.ServiceBus.Core` |
| `ForkForward.Cargo.EntityFrameworkCore` | `MassTransit.EntityFrameworkCore` |
| `ForkForward.Cargo.Testing` | `MassTransit.TestFramework` |

## Moving from MassTransit 8

Replace each `MassTransit.*` package reference with the `ForkForward.Cargo` package in the table. The code does not change: the types, their members and the behaviour are those of MassTransit 8.5.12. The guide [Moving from MassTransit 8](https://forkforward.dev/dotnet-cargo/introduction/moving-from-masstransit/) lists what else to know.

```csharp
using MassTransit;

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<SubmitOrderConsumer>();
    x.UsingRabbitMq((context, cfg) => cfg.ConfigureEndpoints(context));
});
```

## Links

- Documentation: https://forkforward.dev/dotnet-cargo/
- Source: https://github.com/ForkForward/dotnet-cargo
- Licence: Apache License 2.0. Copyright 2007-2024 Chris Patterson; modifications copyright 2026 ForkForward contributors. See LICENSE and NOTICE in the package.
