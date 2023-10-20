using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using laget.Azure.ServiceBus.Wrappers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace laget.Azure.ServiceBus.Topic
{
    public interface ITopicReceiver
    {
        Task RegisterAsync(Func<ProcessMessageEventArgs, ServiceBusMessage, Task> messageHandler, Func<ProcessErrorEventArgs, Task> errorHandler, CancellationToken cancellationToken = default);
        Task StopAsync(CancellationToken cancellationToken = default);
        Task DisposeAsync();
    }

    public class TopicReceiver : ITopicReceiver
    {
        private readonly TopicOptions _topicOptions;

        internal static BlobContainerClient BlobContainerClient;
        internal static ServiceBusClient ServiceBusClient;
        internal static ServiceBusProcessor ServiceBusProcessor;

        public TopicReceiver(string connectionString, TopicOptions topicOptions)
            : this(null, new ServiceBusClient(connectionString, topicOptions.ServiceBusClientOptions), topicOptions)
        { }

        public TopicReceiver(string blobConnectionString, string blobContainer, string connectionString, TopicOptions topicOptions)
            : this(new BlobContainerClient(blobConnectionString, blobContainer), new ServiceBusClient(connectionString, topicOptions.ServiceBusClientOptions), topicOptions)
        { }

        internal TopicReceiver(BlobContainerClient blobContainerClient, ServiceBusClient serviceBusClient, TopicOptions topicOptions)
        {
            BlobContainerClient = blobContainerClient;
            ServiceBusClient = serviceBusClient;
            _topicOptions = topicOptions;
        }

        public async Task RegisterAsync(Func<ProcessMessageEventArgs, ServiceBusMessage, Task> messageHandler, Func<ProcessErrorEventArgs, Task> errorHandler, CancellationToken cancellationToken = default)
        {
            ServiceBusProcessor = ServiceBusClient.CreateProcessor(_topicOptions.TopicName, _topicOptions.SubscriptionName, _topicOptions.ServiceBusProcessorOptions);

            ServiceBusProcessor.ProcessMessageAsync += new MessageHandlerWrapper(BlobContainerClient, _topicOptions.TopicName).Handler(messageHandler);
            ServiceBusProcessor.ProcessErrorAsync += errorHandler;

            await ServiceBusProcessor.StartProcessingAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            await ServiceBusProcessor.StopProcessingAsync(cancellationToken);
        }

        public async Task DisposeAsync()
        {
            await ServiceBusProcessor.DisposeAsync();
            await ServiceBusClient.DisposeAsync();
        }
    }
}
