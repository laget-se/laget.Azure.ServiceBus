using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using laget.Azure.ServiceBus.Factories;
using System;
using System.Threading.Tasks;

namespace laget.Azure.ServiceBus.Queue
{
    public interface IQueueReceiver
    {
        Task RegisterAsync(Func<ProcessMessageEventArgs, Task> messageHandler, Func<ProcessErrorEventArgs, Task> errorHandler);
        Task RegisterAsync(Func<BlobClient, ProcessMessageEventArgs, Task> messageHandler, Func<ProcessErrorEventArgs, Task> errorHandler);
    }

    public class QueueReceiver : IQueueReceiver
    {
        private readonly ServiceBusBlobMessageFactory _serviceBusBlobMessageFactory;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly QueueOptions _queueQueueOptions;

        public QueueReceiver(string connectionString, QueueOptions queueOptions)
            : this(new ServiceBusBlobMessageFactory(), new ServiceBusClient(connectionString, queueOptions.ServiceBusClientOptions), queueOptions)
        { }

        public QueueReceiver(string blobConnectionString, string blobContainer, string connectionString, QueueOptions queueOptions)
            : this(new ServiceBusBlobMessageFactory(blobConnectionString, blobContainer, queueOptions.QueueName), new ServiceBusClient(connectionString, queueOptions.ServiceBusClientOptions), queueOptions)
        { }

        internal QueueReceiver(ServiceBusBlobMessageFactory serviceBusBlobMessageFactory, ServiceBusClient serviceBusClient, QueueOptions queueOptions)
        {
            _serviceBusBlobMessageFactory = serviceBusBlobMessageFactory;
            _serviceBusClient = serviceBusClient;
            _queueQueueOptions = queueOptions;
        }

        public async Task RegisterAsync(Func<ProcessMessageEventArgs, Task> messageHandler, Func<ProcessErrorEventArgs, Task> errorHandler)
        {
            var processor = _serviceBusClient.CreateProcessor(_queueQueueOptions.QueueName, _queueQueueOptions.ServiceBusProcessorOptions);

            processor.ProcessMessageAsync += messageHandler;
            processor.ProcessErrorAsync += errorHandler;

            await processor.StartProcessingAsync();
        }

        public async Task RegisterAsync(Func<BlobClient, ProcessMessageEventArgs, Task> messageHandler, Func<ProcessErrorEventArgs, Task> errorHandler)
        {
            var processor = _serviceBusClient.CreateProcessor(_queueQueueOptions.QueueName, _queueQueueOptions.ServiceBusProcessorOptions);

            processor.ProcessMessageAsync += _serviceBusBlobMessageFactory.HandlerWrapper(messageHandler);
            processor.ProcessErrorAsync += errorHandler;

            await processor.StartProcessingAsync();
        }
    }
}
