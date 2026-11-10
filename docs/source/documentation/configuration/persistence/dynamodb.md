# DynamoDb

!!! warning "Not published by ForkForward Cargo yet"

    This page refers to a package that ForkForward Cargo does not publish yet: `MassTransit.DynamoDb`.
    The code is in the repository, but the package name is not decided. Don't add the original MassTransit
    package instead: a project must not reference ForkForward Cargo and MassTransit packages at the same time,
    as both contain `MassTransit.dll`. See [Packages](../../../support/packages.md).

DynamoDB is a fully managed NoSQL database provided by AWS.
Setting it up as a SagaRepository is simple, but do note that it requires implementing the _ISagaVersion_ interface
that requires a _Version_ property for optimistic concurrency:

```csharp
public class OrderState :
    SagaStateMachineInstance,
    ISagaVersion
{
    public Guid CorrelationId { get; set; }
    public int Version { get; set; }
    public string CurrentState { get; set; }
    public DateTime? OrderDate { get; set; }
}
```

## Configuration
In Order to configure DynamoDB as a saga repository, use the *AddMassTransit* container extension:

```csharp
builder.Services.AddMassTransit(cfg =>
{
    cfg.AddSagaStateMachine<OrderStateMachine, OrderState>()
        .DynamoDbRepository(config =>
        {
            // required
            config.TableName = "Orders";
             // required. Refer to AWS SDK docs for how to create a context.
            config.ContextFactory(provider => new DynamoDBContext(dynamoDbClient));
        });
});
```

The code above configures the repository to store saga instances in a table named `Orders`.
A `DynamoDBContext` must be created from an existing `AmazonDynamoDBClient` instance.
For example, if a local dynamodb instance is used, the code creating a client could look like this:

```csharp
var dynamoDbClient = new AmazonDynamoDBClient(new AmazonDynamoDBConfig { ServiceURL = "http://localhost:4566" });
```
