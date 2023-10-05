# laget.Azure.ServiceBus
A generic implementation of Microsoft.Azure.ServiceBus, the next generation Azure Service Bus .NET Standard client library that focuses on queues & topics. For more information about Service Bus...

![Nuget](https://img.shields.io/nuget/v/laget.Azure.ServiceBus)
![Nuget](https://img.shields.io/nuget/dt/laget.Azure.ServiceBus)

## Usage
> Azure Service Bus supports reliable message queuing and durable publish/subscribe messaging. The messaging entities that form the core of the messaging capabilities in Service Bus are queues, topics and subscriptions.

### Topics
> A queue allows processing of a message by a single consumer. In contrast to queues, topics and subscriptions provide a one-to-many form of communication in a publish and subscribe pattern. It's useful for scaling to large numbers of recipients. Each published message is made available to each subscription registered with the topic. Publisher sends a message to a topic and one or more subscribers receive a copy of the message.

#### TopicSender
```c#
public class SomeClass
{
    readonly TopicSender _sender;
    
    public SomeClass(IConfiguration configuration)
    {
        _sender = new TopicSender(
            configuration.GetConnectionString("AzureServiceBus"),
            new TopicOptions
            {
                TopicName = "topic"
            });
    }

    public async Task SendAsync(Models.Event @event)
    {
        await _sender.SendAsync(@event);
    }
}
```

> We're supporting large messages by persisting the message data as `blob` in a `Azure Storage Account`.
```c#
public class SomeClass
{
    readonly TopicSender _sender;
    
    public SomeClass(IConfiguration configuration)
    {
        _sender = new TopicSender(
            configuration.GetConnectionString("AzureStorageAccount"),
            configuration.GetValue<string>("AzureStorageAccount:BlobContainerName"),
            configuration.GetConnectionString("AzureServiceBus"),
            new TopicOptions
            {
                TopicName = "topic"
            });
    }

    public async Task SendAsync(Models.Event @event)
    {
        await _sender.SendAsync(@event);
    }
}
```

#### TopicReceiver
```c#
public class SomeClass : IHostedService
{
    readonly TopicReceiver _receiver;

    public SomeClass(IConfiguration configuration)
    {
        _receiver = new TopicReceiver(
            configuration.GetConnectionString("AzureServiceBus"),
            new TopicOptions
            {
                TopicName = "topic",
                SubscriptionName = "subscription"
            });
    }

    public async Task StartAsync(CancellationToken ct)
    {
        await _receiver.RegisterAsync(MessageHandler, ErrorHandler);
    }

    public Task StopAsync(CancellationToken ct)
    {
        return Task.CompletedTask;
    }

    private async Task MessageHandler(ProcessMessageEventArgs args, ServiceBusMessage message)
    {
        var model = message.Deserialize<Models.Message>();
        return Task.CompletedTask;
    }

    private static Task ErrorHandler(ProcessErrorEventArgs args)
    {
        return Task.CompletedTask;
    }
}
```

> We're supporting large messages by persisting the message data as `blob` in a `Azure Storage Account`.
```c#
public class SomeClass : IHostedService
{
    readonly TopicReceiver _receiver;

    public SomeClass(IConfiguration configuration)
    {
        _receiver = new TopicReceiver(
            configuration.GetConnectionString("AzureStorageAccount"),
            configuration.GetValue<string>("AzureStorageAccount:BlobContainerName"),
            configuration.GetConnectionString("AzureServiceBus"),
            new TopicOptions
            {
                TopicName = "topic",
                SubscriptionName = "subscription"
            });
    }

    public async Task StartAsync(CancellationToken ct)
    {
        await _receiver.RegisterAsync(MessageHandler, ErrorHandler);
    }

    public Task StopAsync(CancellationToken ct)
    {
        return Task.CompletedTask;
    }

    private async Task MessageHandler(ProcessMessageEventArgs args, ServiceBusMessage message)
    {
        var model = message.Deserialize<Models.Message>();
        return Task.CompletedTask;
    }

    private static Task ErrorHandler(ProcessErrorEventArgs args)
    {
        return Task.CompletedTask;
    }
}
```

### Queues
> Queues offer First In, First Out (FIFO) message delivery to one or more competing consumers. That is, receivers typically receive and process messages in the order in which they were added to the queue. And, only one message consumer receives and processes each message.

#### QueueSender
```c#
public class SomeClass
{
    readonly QueueSender _sender;
    
    public SomeClass(IConfiguration configuration)
    {
        _sender = new QueueSender(configuration.GetConnectionString("AzureServiceBus"),
            new TopicOptions
            {
                QueueName = "queue"
            });
    }

    public async Task SendAsync(Models.Event @event)
    {
        await _sender.SendAsync(@event);
    }
}
```

> We're supporting large messages by persisting the message data as `blob` in a `Azure Storage Account`.
```c#
public class SomeClass
{
    readonly QueueSender _sender;
    
    public SomeClass(IConfiguration configuration)
    {
        _sender = new QueueSender(
            configuration.GetConnectionString("AzureStorageAccount"),
            configuration.GetValue<string>("AzureStorageAccount:BlobContainerName"),
            configuration.GetConnectionString("AzureServiceBus"),
            new TopicOptions
            {
                QueueName = "queue"
            });
    }

    public async Task SendAsync(Models.Event @event)
    {
        await _sender.SendAsync(@event);
    }
}
```

#### QueueReceiver
```c#
public class SomeClass : IHostedService
{
    readonly QueueReceiver _receiver;

    public SomeClass(IConfiguration configuration)
    {
        _receiver = new QueueReceiver(
            configuration.GetConnectionString("AzureServiceBus"),
            new QueueReceiver
            {
                QueueName = "queue"
            });
    }

    public async Task StartAsync(CancellationToken ct)
    {
        await _receiver.RegisterAsync(MessageHandler, ErrorHandler);
    }

    public Task StopAsync(CancellationToken ct)
    {
        return Task.CompletedTask;
    }

    private async Task MessageHandler(ProcessMessageEventArgs args, ServiceBusMessage message)
    {
        var model = message.Deserialize<Models.Message>();
        return Task.CompletedTask;
    }

    private static Task ErrorHandler(ProcessErrorEventArgs args)
    {
        return Task.CompletedTask;
    }
}
```

> We're supporting large messages by persisting the message data as `blob` in a `Azure Storage Account`.
```c#
public class SomeClass : IHostedService
{
    readonly QueueReceiver _receiver;

    public SomeClass(IConfiguration configuration)
    {
        _receiver = new QueueReceiver(
            configuration.GetConnectionString("AzureStorageAccount"),
            configuration.GetValue<string>("AzureStorageAccount:BlobContainerName"),
            configuration.GetConnectionString("AzureServiceBus"),
            new QueueReceiver
            {
                QueueName = "queue"
            });
    }

    public async Task StartAsync(CancellationToken ct)
    {
        await _receiver.RegisterAsync(MessageHandler, ErrorHandler);
    }

    public Task StopAsync(CancellationToken ct)
    {
        return Task.CompletedTask;
    }

    private async Task MessageHandler(ProcessMessageEventArgs args, ServiceBusMessage message)
    {
        var model = message.Deserialize<Models.Message>();
        return Task.CompletedTask;
    }

    private static Task ErrorHandler(ProcessErrorEventArgs args)
    {
        return Task.CompletedTask;
    }
}
```