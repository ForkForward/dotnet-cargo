---
title: Packages
---

# Packages

ForkForward Cargo is published on [nuget.org](https://www.nuget.org/packages?q=ForkForward.Cargo) under the
`ForkForward.Cargo` names. The `MassTransit` packages on nuget.org belong to the original authors, not to this fork.

## Published

| Package | Continues | Purpose |
|---|---|---|
| [ForkForward.Cargo](https://www.nuget.org/packages/ForkForward.Cargo) | `MassTransit` | Main package |
| [ForkForward.Cargo.Abstractions](https://www.nuget.org/packages/ForkForward.Cargo.Abstractions) | `MassTransit.Abstractions` | Abstractions |
| [ForkForward.Cargo.RabbitMQ](https://www.nuget.org/packages/ForkForward.Cargo.RabbitMQ) | `MassTransit.RabbitMQ` | RabbitMQ transport |
| [ForkForward.Cargo.AzureServiceBus](https://www.nuget.org/packages/ForkForward.Cargo.AzureServiceBus) | `MassTransit.Azure.ServiceBus.Core` | Azure Service Bus transport |
| [ForkForward.Cargo.EntityFrameworkCore](https://www.nuget.org/packages/ForkForward.Cargo.EntityFrameworkCore) | `MassTransit.EntityFrameworkCore` | Entity Framework Core integration |
| [ForkForward.Cargo.Testing](https://www.nuget.org/packages/ForkForward.Cargo.Testing) | `MassTransit.TestFramework` | Testing tools |

Only previews are published so far: add `--prerelease` to `dotnet add package`.

## Not published yet

The code of these packages is in the repository, but their names are not decided, so they are not published. A
project that needs one of them cannot move to ForkForward Cargo yet: it must not reference ForkForward Cargo and the
original MassTransit packages at the same time, because both contain `MassTransit.dll`.

### Transports

- `MassTransit.ActiveMQ`
- `MassTransit.AmazonSQS`
- `MassTransit.SqlTransport.PostgreSQL`
- `MassTransit.SqlTransport.SqlServer`
- `MassTransit.WebJobs.ServiceBus` and `MassTransit.WebJobs.EventHubs`, for Azure Functions

### Riders

- `MassTransit.EventHub`
- `MassTransit.Kafka`

### Saga persistence

- `MassTransit.Azure.Cosmos`
- `MassTransit.Azure.Cosmos.Table`
- `MassTransit.DapperIntegration`
- `MassTransit.DynamoDb`
- `MassTransit.EntityFramework`, for Entity Framework 6
- `MassTransit.Marten`
- `MassTransit.MongoDb`
- `MassTransit.NHibernate`
- `MassTransit.Redis`

### Message data

- `MassTransit.AmazonS3`
- `MassTransit.Azure.Storage`

### Scheduling

- `MassTransit.Hangfire`
- `MassTransit.Quartz`

### Interoperability

- `MassTransit.Interop.NServiceBus`
- `MassTransit.MessagePack`
- `MassTransit.Newtonsoft`

### Other

- `MassTransit.Analyzers`
- `MassTransit.SignalR`
- `MassTransit.StateMachineVisualizer`

The `dotnet new` templates, `MassTransit.Templates`, are not part of this repository.

## Deprecated packages

The following packages from earlier versions of MassTransit are no longer supported.

* Automatonymous
* Automatonymous.NHibernate
* Automatonymous.Visualizer
* GreenPipes
* MassTransit.ApplicationInsights
* MassTransit.AspNetCore
* MassTransit.Autofac
* MassTransit.Automatonymous
* MassTransit.Automatonymous.Autofac
* MassTransit.Automatonymous.Extensions.DependencyInjection
* MassTransit.Automatonymous.Lamar
* MassTransit.Automatonymous.SimpleInjector
* MassTransit.Automatonymous.StructureMap
* MassTransit.Automatonymous.Windsor
* MassTransit.AzureServiceBus
* MassTransit.CastleWindsor
* MassTransit.Extensions.DependencyInjection
* MassTransit.Extensions.Logging
* MassTransit.Host
* MassTransit.Http
* MassTransit.Lamar
* MassTransit.Log4Net
* MassTransit.MSMQ
* MassTransit.Ninject
* MassTransit.NLog
* MassTransit.Platform.Abstractions
* MassTransit.Prometheus
* MassTransit.Reactive
* MassTransit.SerilogIntegration
* MassTransit.SimpleInjector
* MassTransit.StructureMap
* MassTransit.StructureMapSigned
* MassTransit.Unity
