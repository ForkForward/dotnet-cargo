---
title: Introduction
description: What is MassTransit
---

# What is MassTransit?

MassTransit is an open-source distributed application framework for .NET that provides a consistent abstraction on top of the supported message transports. The interfaces provided by MassTransit reduce message-based application complexity and allow developers to focus their effort on adding business value.

## What does it do for you?

<div class="grid cards" markdown>

-   :lucide-route:{ .lg .middle } **Message Routing**

    ---

    Type-based publish/subscribe and automatic broker topology configuration

-   :lucide-triangle-alert:{ .lg .middle } **Exception Handling**

    ---

    When an exception is thrown, messages can be retried, redelivered, or moved to an *error* queue

-   :lucide-flask-conical:{ .lg .middle } **Test Harness**

    ---

    Fast, in-memory unit tests with consumed, published, and sent message observers

-   :lucide-telescope:{ .lg .middle } **Observability**

    ---

    Native Open Telemetry (OTEL) support for end-to-end activity tracing

-   :lucide-package:{ .lg .middle } **Dependency Injection**

    ---

    Service collection configuration and scope service provider management

-   :lucide-alarm-clock:{ .lg .middle } **Scheduling**

    ---

    Schedule message delivery using transport delay, Quartz.NET, and Hangfire

-   :lucide-workflow:{ .lg .middle } **Sagas, State Machines**

    ---

    Reliable, durable, event-driven workflow orchestration

-   :lucide-list-checks:{ .lg .middle } **Routing Slip Activities**

    ---

    Distributed, fault-tolerant transaction choreography with compensation

-   :lucide-arrow-left-right:{ .lg .middle } **Request, Response**

    ---

    Handle requests with fast, automatic response routing

</div>
