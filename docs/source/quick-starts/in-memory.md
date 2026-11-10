# In Memory

This tutorial will get you from zero to up and running with [In Memory](../documentation/transports/in-memory.md) and MassTransit.

:fontawesome-brands-youtube: [Watch the video on YouTube](https://www.youtube.com/watch?v=WjOX1DrzO-w)

## Prerequisites

This example requires a functioning installation of the .NET Runtime and SDK (at least 6.0).

## Install the Templates

MassTransit includes project and item [templates](templates.md) simplifying the creation of new projects. Install the templates by executing the command below in the console. A video introducing the templates is available on [YouTube](https://youtu.be/nYKq61-DFBQ).

```
dotnet new install MassTransit.Templates
```

## Create the Project

To create a service using MassTransit, create a worker via the Command Prompt.

```bash
$ dotnet new mtworker -n GettingStarted
$ cd GettingStarted
$ dotnet new mtconsumer
```

## Use the ForkForward Cargo Packages

The templates come from the MassTransit project, and the worker they create references the original `MassTransit`
packages. Open `GettingStarted.csproj`, remove every `PackageReference` whose name starts with `MassTransit`, and add
ForkForward Cargo instead:

```bash
$ dotnet remove package MassTransit
$ dotnet add package ForkForward.Cargo --prerelease
```

The code stays the same: the namespace is still `MassTransit`. See
[Moving from MassTransit 8](../introduction/moving-from-masstransit.md) for the names of the other packages.

## Project Overview

When you open the project you will see that you have 1 class file.

- `Program.cs` is the standard entry point and here we configure the host builder.

## Create a Contract

Create a `Contracts` folder in the root of your project, and within that folder create a file named `GettingStarted.cs` with the following contents:

```csharp
namespace GettingStarted.Contracts;
public record GettingStarted() 
{
    public string Value { get; init; }
}
```

## Add a Background Service

In the root of the project add `Worker.cs`

```csharp
namespace GettingStarted;

using System;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using MassTransit;
using Microsoft.Extensions.Hosting;

public class Worker : BackgroundService
{
    readonly IBus _bus;

    public Worker(IBus bus)
    {
        _bus = bus;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _bus.Publish(new GettingStarted { Value = $"The time is {DateTimeOffset.Now}" }, stoppingToken);

            await Task.Delay(1000, stoppingToken);
        }
    }
}
```

## Register the Service

In `Program.cs` at the bottom of the `ConfigureServices` method add

```csharp
services.AddHostedService<Worker>();
```

## Create a Consumer

Create a `Consumers` folder in the root of your project, and within that folder create a file named `GettingStartedConsumer.cs` with the following contents:

```csharp
namespace GettingStarted.Consumers;

using System.Threading.Tasks;
using Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;

public class GettingStartedConsumer :
    IConsumer<GettingStarted>
{
    readonly ILogger<GettingStartedConsumer> _logger;

    public GettingStartedConsumer(ILogger<GettingStartedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<GettingStarted> context)
    {
        _logger.LogInformation("Received Text: {Text}", context.Message.Value);
        return Task.CompletedTask;
    }
}
```

## Run the Project

```bash
$ dotnet run
```

The output should have changed to show the message consumer generating the output (again, press Control+C to exit).

```
Building...
info: MassTransit[0]
      Configured endpoint Message, Consumer: GettingStarted.MessageConsumer
info: MassTransit[0]
      Bus started: loopback://localhost/
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Development
info: Microsoft.Hosting.Lifetime[0]
      Content root path: /Users/chris/Garbage/start/GettingStarted
info: GettingStarted.MessageConsumer[0]
      Received Text: The time is 3/24/2021 12:02:01 PM -05:00
info: GettingStarted.MessageConsumer[0]
      Received Text: The time is 3/24/2021 12:02:02 PM -05:00
```
