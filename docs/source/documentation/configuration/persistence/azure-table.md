# Azure Table Storage

!!! warning "Not published by ForkForward Cargo yet"

    This page refers to a package that ForkForward Cargo does not publish yet: `MassTransit.Azure.Cosmos.Table`.
    The code is in the repository, but the package name is not decided. Don't add the original MassTransit
    package instead: a project must not reference ForkForward Cargo and MassTransit packages at the same time,
    as both contain `MassTransit.dll`. See [Packages](../../../support/packages.md).

Azure Tables are exposed in two ways in Azure - via Storage accounts & via the premium offering within Cosmos DB APIs. This persistence supports both implementations and behind the curtains uses the Azure.Data.Tables library for communication.

!!! success

    Azure Tables currently only supports Optimistic Concurrency. Mass Transit manages the ETag property in Payload Context and uses this property for state machine updates. Concurrency errors can be spotted in logs via standard "Precondition Failed" errors from Table Storage.

!!! warning

    Be sure to set DateTime properties as nullable when updated later in the saga. Failure to do this can result in 400 bad requests from Table Storage.

```csharp
public class OrderState :
    SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }

    public DateTime? OrderDate { get; set; }
}
```

## Container Integration

To configure a Table as the saga repository for a saga, use the code shown below using the _AddMassTransit_ container extension.

```csharp
TableClient cloudTable;
container.AddMassTransit(cfg =>
{
    cfg.AddSagaStateMachine<OrderStateMachine, OrderState>()
        .AzureTableRepository(endpointUri, key, r =>
        {
           cfg.ConnectionFactory(() => cloudTable);
        });
});
```

The container extension will register the saga repository in the container.

To configure the saga repository with a specific key formatter, use the code shown below with _KeyFormatter_ configuration extension.

```csharp
TableClient cloudTable;
container.AddMassTransit(cfg =>
{
    cfg.AddSagaStateMachine<OrderStateMachine, OrderState>()
        .AzureTableRepository(endpointUri, key, r =>
        {
           cfg.ConnectionFactory(() => cloudTable);
           cfg.KeyFormatter(() => new ConstRowSagaKeyFormatter<OrderState>(typeof(OrderState).Name)))
        });
});
```

Unlike the default `ConstPartitionSagaKeyFormatter`, `ConstRowSagaKeyFormatter` in this example uses `PartitionKey` to store the correlationId which may benefit from [scale-out capability of Tables](https://docs.microsoft.com/en-us/rest/api/storageservices/designing-a-scalable-partitioning-strategy-for-azure-table-storage#scalability).
