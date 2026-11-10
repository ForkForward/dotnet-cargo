# ActiveMQ Transport

!!! warning "Not published by ForkForward Cargo yet"

    This page refers to a package that ForkForward Cargo does not publish yet: `MassTransit.ActiveMQ`.
    The code is in the repository, but the package name is not decided. Don't add the original MassTransit
    package instead: a project must not reference ForkForward Cargo and MassTransit packages at the same time,
    as both contain `MassTransit.dll`. See [Packages](../../support/packages.md).

ActiveMQ is an open-source message broker software that implements the Java Message Service (JMS) API, as well as other protocols such as Advanced Message Queuing Protocol (AMQP), Streaming Text Oriented Messaging Protocol (STOMP), and MQ Telemetry Transport (MQTT). It is written in Java and is built on the Apache ActiveMQ project, which provides a high-performance, reliable, and scalable messaging infrastructure.

ActiveMQ can be used to decouple and distribute systems by sending messages between them. It supports a variety of messaging patterns, including point-to-point, publish/subscribe, and request/response. It also provides features such as message persistence, load balancing, and clustering, which allows for high availability and scalability.

ActiveMQ also has a built-in management console, which allows for monitoring and management of the broker, queues, and connections. It also supports various plugins, such as the Apache Camel plugin, that provide additional functionality.

## ActiveMQ Artemis

ActiveMQ Artemis is a high-performance, open-source message broker that is a next generation of the ActiveMQ project. It is designed to handle large numbers of concurrent clients and large volumes of messages, while providing low latency and high throughput. It is written in Java and is built on the HornetQ project, which provides a high-performance, reliable, and scalable messaging infrastructure.
