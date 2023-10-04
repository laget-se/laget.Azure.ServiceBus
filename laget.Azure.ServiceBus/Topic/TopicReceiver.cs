using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using laget.Azure.ServiceBus.Factories;
using System;
using System.Threading.Tasks;

namespace laget.Azure.ServiceBus.Topic
{
    public interface ITopicReceiver
    {
        Task RegisterAsync(Func<ProcessMessageEventArgs, Task> messageHandler, Func<ProcessErrorEventArgs, Task> errorHandler);
        Task RegisterAsync(Func<BlobClient, ProcessMessageEventArgs, Task> messageHandler, Func<ProcessErrorEventArgs, Task> errorHandler);
    }

    public class TopicReceiver : ITopicReceiver
    {
        private readonly ServiceBusBlobMessageFactory _serviceBusBlobMessageFactory;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly TopicOptions _topicOptions;

        public TopicReceiver(string connectionString, TopicOptions topicOptions)
            : this(new ServiceBusBlobMessageFactory(), new ServiceBusClient(connectionString, topicOptions.ServiceBusClientOptions), topicOptions)
        { }

        public TopicReceiver(string blobConnectionString, string blobContainer, string connectionString, TopicOptions topicOptions)
            : this(new ServiceBusBlobMessageFactory(blobConnectionString, blobContainer, topicOptions.TopicName), new ServiceBusClient(connectionString, topicOptions.ServiceBusClientOptions), topicOptions)
        { }

        internal TopicReceiver(ServiceBusBlobMessageFactory serviceBusBlobMessageFactory, ServiceBusClient serviceBusClient, TopicOptions topicOptions)
        {
            _serviceBusBlobMessageFactory = serviceBusBlobMessageFactory;
            _serviceBusClient = serviceBusClient;
            _topicOptions = topicOptions;
        }

        public async Task RegisterAsync(Func<ProcessMessageEventArgs, Task> messageHandler, Func<ProcessErrorEventArgs, Task> errorHandler)
        {
            var processor = _serviceBusClient.CreateProcessor(_topicOptions.TopicName, _topicOptions.SubscriptionName, _topicOptions.ServiceBusProcessorOptions);

            processor.ProcessMessageAsync += messageHandler;
            processor.ProcessErrorAsync += errorHandler;

            await processor.StartProcessingAsync();
        }

        public async Task RegisterAsync(Func<BlobClient, ProcessMessageEventArgs, Task> messageHandler, Func<ProcessErrorEventArgs, Task> errorHandler)
        {
            var processor = _serviceBusClient.CreateProcessor(_topicOptions.TopicName, _topicOptions.SubscriptionName, _topicOptions.ServiceBusProcessorOptions);

            processor.ProcessMessageAsync += _serviceBusBlobMessageFactory.HandlerWrapper(messageHandler);
            processor.ProcessErrorAsync += errorHandler;

            await processor.StartProcessingAsync();
        }
    }
}
