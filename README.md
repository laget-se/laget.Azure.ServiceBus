# laget.Azure.ServiceBus
A generic implementation of Microsoft.Azure.ServiceBus, the next generation Azure Service Bus .NET Standard client library that focuses on queues & topics. For more information about Service Bus...

![Nuget](https://img.shields.io/nuget/v/laget.Azure.ServiceBus)
![Nuget](https://img.shields.io/nuget/dt/laget.Azure.ServiceBus)

## Usage
### TopicSender
```c#
public class SomeClass : IHostedService
{
    readonly TopicSender _sender;
    
    public SomeClass(IConfiguration configuration)
    {
        _topicSender = new TopicSender(connectionString,
            new TopicOptions
            {
                TopicName = configuration.GetValue<string>("ServiceBus:TopicName"),
                RetryPolicy = new RetryExponential(minimumBackoff: TimeSpan.FromSeconds(5), maximumBackoff: TimeSpan.FromMinutes(5), maximumRetryCount: 100)
            });
    }

    public async Task SendAsync(Models.Event @event)
    {
        var json = JsonConvert.SerializeObject(@event, new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy
                {
                    ProcessDictionaryKeys = true,
                    ProcessExtensionDataNames = true,
                    OverrideSpecifiedNames = true
                }
            },
            Formatting = Formatting.Indented
        });

        await _sender.SendAsync(json);
    }
}
```

### TopicReceiver
```c#
public class SomeClass : IHostedService
{
    readonly TopicReceiver _receiver;

    public SomeClass(IConfiguration configuration)
    {
        _receiver = new TopicReceiver(configuration.GetValue<string>("ServiceBus:Url")
                .Replace("{name}", configuration.GetValue<string>("ServiceBus:QueueService:Name"))
                .Replace("{key}", configuration.GetValue<string>("ServiceBus:QueueService:QueueKey")),
            new TopicOptions
            {
                TopicName = "queue",
                SubscriptionName = "queue-service",
                ReceiveMode = ReceiveMode.PeekLock
            });
    }


    public async Task StartAsync(CancellationToken ct)
    {
        await Task.Run(() =>
        {
            _receiver.Register((message, _) =>
            {
                var queue = message.Deserialize<QueueMessage>();

                _handler.Handle(queue);

                return Task.CompletedTask;
            }, ExceptionHandler);
        }, ct);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    Task ExceptionHandler(ExceptionReceivedEventArgs ex)
    {
        // Log here

        return Task.CompletedTask;
    }
}
```
