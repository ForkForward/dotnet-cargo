# Roslyn Analyzer

!!! warning "Not published by ForkForward Cargo yet"

    This page refers to a package that ForkForward Cargo does not publish yet: `MassTransit.Analyzers`.
    The code is in the repository, but the package name is not decided. Don't add the original MassTransit
    package instead: a project must not reference ForkForward Cargo and MassTransit packages at the same time,
    as both contain `MassTransit.dll`. See [Packages](../../../support/packages.md).

MassTransit has a code analyzer which detects and provides code fixes which can be helpful to identify potential issues.

## Message Initializers

[Message Initializers](../../concepts/producers.md#message-initialization) are used to initialize a message without having to create a backing class.

The analyzer supports methods that accept an `object` _values_ argument, including:

- `ISendEndpoint.Send<T>(object values)`
- `IPublishEndpoint.Publish<T>(object values)`
- `ConsumeContext.RespondAsync<T>(object values)`

## ConsumeContext CancellationToken

The analyzer is similar to [CA2016](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca2016), but it is able to recognize when `CancellationToken` is present from `ConsumeContext` or any its implementation.
