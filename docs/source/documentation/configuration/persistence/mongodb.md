# MongoDB

!!! warning "Not published by ForkForward Cargo yet"

    This page refers to a package that ForkForward Cargo does not publish yet: `MassTransit.MongoDb`.
    The code is in the repository, but the package name is not decided. Don't add the original MassTransit
    package instead: a project must not reference ForkForward Cargo and MassTransit packages at the same time,
    as both contain `MassTransit.dll`. See [Packages](../../../support/packages.md).

MongoDB is easy to setup as a saga repository. MassTransit includes sensible defaults, and there is no need to explicitly map sagas.

Storing a saga in MongoDB requires an additional interface, _ISagaVersion_, which has a _Version_ property used for optimistic concurrency. An example saga is shown below.

```csharp
public class OrderState :
    SagaStateMachineInstance,
    ISagaVersion
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }

    public DateTime? OrderDate { get; set; }

    public int Version { get; set; }
}
```

## Configuration

To configure MongoDB as a saga repository, use the code shown below using the _AddMassTransit_ container extension. This will configure MongoDB to connect to the local MongoDB instance on the default port using Optimistic concurrency. The _CorrelationId_ property will be automatically mapped to be the document identifier.

```csharp
services.AddMassTransit(x =>
{
    x.AddSagaStateMachine<OrderStateMachine, OrderState>()
        .MongoDbRepository(r =>
        {
            r.Connection = "mongodb://127.0.0.1";
            r.DatabaseName = "orderdb";
        });
});
```

In the example above, saga instances are stored in a collection named `order.states`. The collection name can be specified using the _CollectionName_ property. Alternatively, a collection name formatter can be specified using the _CollectionNameFormatter_ method.

```csharp
.MongoDbRepository(r =>
{
    r.Connection = "mongodb://127.0.0.1";
    r.DatabaseName = "orderdb";

    r.CollectionName = "orders";
});
```

Container integration gives you ability to configure class map based on saga type. You can use `Action<BsonClassMap>` explicitly:

```csharp
.MongoDbRepository(r =>
{
    r.Connection = "mongodb://127.0.0.1";
    r.DatabaseName = "orderdb";

    r.ClassMap(m => {});
});
```

`BsonClassMap<TSaga>` registered inside container will be used by default for `TSaga` configuration:

```csharp
class OrderStateClassMap :
    BsonClassMap<OrderState>
{
    public OrderStateClassMap()
    {
        MapProperty(x => x.OrderDate)
            .SetSerializer(new DateTimeSerializer(DateTimeKind.Utc));
    }
}

services.AddSingleton<BsonClassMap<OrderState>, OrderStateClassMap>();
```
