# ForkForward Cargo

**ForkForward Cargo is the open-source continuation of [MassTransit](https://github.com/MassTransit/MassTransit) 8**, the distributed application framework for .NET created by Chris Patterson and Dru Sellers. It continues the last Apache 2.0-licensed code of MassTransit and keeps it free, open source and building on current .NET. Changes in this fork are written by AI coding tools and automation; people review them and are responsible for releases.

Messages are the cargo: the framework carries them between services over RabbitMQ, Azure Service Bus, Amazon SQS, Kafka and the other supported transports, and handles consumers, sagas, retries and the outbox on the way.

> [!IMPORTANT]
> This is **not** the official MassTransit. ForkForward is an independent project. It is not affiliated with, sponsored by, or endorsed by Chris Patterson, Massient, Inc., or the MassTransit project. The name "MassTransit" appears here only to say where the code comes from.
> The official MassTransit is at [masstransit.io](https://masstransit.io).

## Where the code comes from

| | |
|---|---|
| Original project | MassTransit, by Chris Patterson, Dru Sellers and the MassTransit contributors |
| Source repository | [MassTransit/MassTransit](https://github.com/MassTransit/MassTransit) |
| Forked from | commit [`d9f61f76`](https://github.com/MassTransit/MassTransit/commit/d9f61f764c802785cc9a4be92547d8f5b63018d4) of 2026-10-09, which is release `v8.5.12` |
| Licence of the original code | Apache License 2.0, Copyright 2007-2024 Chris Patterson |
| Licence of this fork | Apache License 2.0, unchanged ([LICENSE](LICENSE)); the original [NOTICE](NOTICE) is kept |
| What this fork changed | `git log d9f61f76..HEAD` |

All the design, the API and nearly all of the code are the work of the original authors. This fork adds maintenance on top.

## Why this fork exists

The official MassTransit continues with version 9, which Massient, Inc. publishes under a commercial licence ([announcement](https://masstransit.io/introduction/v9-announcement)). Version 8 was published under the Apache License 2.0. That licence lets anyone keep the code alive and open, and that is what this fork does: it continues the 8.x line as open source and keeps it compiling, tested and compatible as .NET moves on.

If you want MassTransit 9, new features from the original authors, or commercial support, use the official MassTransit.

## How this fork is developed

- Changes are produced with AI coding tools and automation such as Dependabot.
- A person reviews every change before it is merged and decides what gets released.
- Nothing is taken from MassTransit 9, from any other code published under the commercial licence, or from any source whose licence does not allow it.
- The original copyright, licence and NOTICE texts stay in place, as the Apache License 2.0 requires.

## Status

- The fork has just been created. The code is MassTransit 8.5.12 unchanged: only the package names are new; the assemblies and namespaces are still called `MassTransit`.
- **Preview packages** are published to nuget.org from the `develop` branch, with versions such as `8.6.0-preview.0.12`. Stable releases will be published from version tags such as `v8.6.0`. The `MassTransit` packages on nuget.org belong to the original authors, not to this fork.
- A preview package contains `MassTransit.dll` signed with the original key, so a project must not reference ForkForward Cargo and the original `MassTransit` packages at the same time.
- The project is published under its own name:

  | | Name |
  |---|---|
  | Product | ForkForward Cargo |
  | Repository | [ForkForward/dotnet-cargo](https://github.com/ForkForward/dotnet-cargo) |
  | Packages | `ForkForward.Cargo.*` |

  The packages, and the MassTransit packages they continue:

  | Package | Continues | Purpose |
  |---|---|---|
  | `ForkForward.Cargo` | `MassTransit` | Main package |
  | `ForkForward.Cargo.Abstractions` | `MassTransit.Abstractions` | Abstractions |
  | `ForkForward.Cargo.RabbitMQ` | `MassTransit.RabbitMQ` | RabbitMQ transport |
  | `ForkForward.Cargo.AzureServiceBus` | `MassTransit.Azure.ServiceBus.Core` | Azure Service Bus transport |
  | `ForkForward.Cargo.EntityFrameworkCore` | `MassTransit.EntityFrameworkCore` | Entity Framework Core integration |
  | `ForkForward.Cargo.Testing` | `MassTransit.TestFramework` | Testing tools |

  Not decided yet: the names of the other packages (the remaining transports, persistence, scheduling and riders), which are not published until then, and whether the namespaces and assembly names change together with the package names.
- The main packages target .NET 8, 9 and 10, .NET Standard 2.0 and .NET Framework 4.7.2, as MassTransit 8.5.12 does.
- Fixes made only in MassTransit 9 are not in this code. Where one is needed here, it is written independently, from the public advisory and this code.

## Using it

Replace each `MassTransit.*` package reference with the `ForkForward.Cargo` package from the table above:

```
dotnet add package ForkForward.Cargo.RabbitMQ --prerelease
```

The code stays exactly as it is with MassTransit 8:

```csharp
using MassTransit;

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<SubmitOrderConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});

public record SubmitOrder(Guid OrderId);

public class SubmitOrderConsumer : IConsumer<SubmitOrder>
{
    public Task Consume(ConsumeContext<SubmitOrder> context)
    {
        Console.WriteLine($"Order submitted: {context.Message.OrderId}");
        return Task.CompletedTask;
    }
}
```

## Documentation

The documentation is published at [forkforward.dev/dotnet-cargo](https://forkforward.dev/dotnet-cargo/). It is the MassTransit 8 documentation, inherited from the original project, with the names of the published packages updated; start with [Moving from MassTransit 8](https://forkforward.dev/dotnet-cargo/introduction/moving-from-masstransit/). Its source is in [docs/source](docs/source), and the Docs workflow publishes it on every change to `develop`. The pages at masstransit.io belong to the official MassTransit and may describe version 9, whose features are not in this fork.

## Building

You need the .NET 10 SDK. The tests target `net9.0`, so running them also needs the .NET 9 runtime.

Clone with `autocrlf` turned off for the repository, then build:

```bash
git clone -c core.autocrlf=false https://github.com/ForkForward/dotnet-cargo.git
cd dotnet-cargo
dotnet build MassTransit.sln
```

## Reporting problems and contributing

Open an issue in this repository for bugs in this fork. Do not report them to the MassTransit project or to Massient; they are not responsible for this code.

To contribute, open a pull request against `develop`.

## Licence and credits

ForkForward Cargo is released under the [Apache License 2.0](LICENSE), the same licence the original code was published under.

MassTransit is Copyright 2007-2024 Chris Patterson and other contributors. Thank you to Chris Patterson, Dru Sellers and everyone who built MassTransit and published it as open source. Modifications made in this fork are Copyright 2026 ForkForward contributors and are released under the same Apache 2.0 terms. The original notice is in [NOTICE](NOTICE).

"MassTransit" and the MassTransit logo, designed by The Agile Badger, belong to their owners. The Apache License 2.0 does not grant permission to use them as trademarks; this fork uses the name only to say where the code comes from, and does not use the logo as its own.
