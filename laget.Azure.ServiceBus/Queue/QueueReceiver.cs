using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using laget.Azure.ServiceBus.Wrappers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace laget.Azure.ServiceBus.Queue
{
    public interface IQueueReceiver
    {
        Task RegisterAsync(Func<ProcessMessageEventArgs, ServiceBusMessage, Task> messageHandler, Func<ProcessErrorEventArgs, Task> errorHandler, CancellationToken cancellationToken = default);
    }

    public class QueueReceiver : IQueueReceiver
    {
        private readonly BlobContainerClient _blobContainerClient;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly QueueOptions _queueQueueOptions;

        public QueueReceiver(string connectionString, QueueOptions queueOptions)
            : this(null, new ServiceBusClient(connectionString, queueOptions.ServiceBusClientOptions), queueOptions)
        { }

        public QueueReceiver(string blobConnectionString, string blobContainer, string connectionString, QueueOptions queueOptions)
            : this(new BlobContainerClient(blobConnectionString, blobContainer), new ServiceBusClient(connectionString, queueOptions.ServiceBusClientOptions), queueOptions)
        { }

        internal QueueReceiver(BlobContainerClient blobContainerClient, ServiceBusClient serviceBusClient, QueueOptions queueOptions)
        {
            _blobContainerClient = blobContainerClient;
            _serviceBusClient = serviceBusClient;
            _queueQueueOptions = queueOptions;
        }

        public async Task RegisterAsync(Func<ProcessMessageEventArgs, ServiceBusMessage, Task> messageHandler, Func<ProcessErrorEventArgs, Task> errorHandler, CancellationToken cancellationToken = default)
        {
            var processor = _serviceBusClient.CreateProcessor(_queueQueueOptions.QueueName, _queueQueueOptions.ServiceBusProcessorOptions);

            processor.ProcessMessageAsync += new MessageHandlerWrapper(_blobContainerClient, _queueQueueOptions.QueueName).Handler(messageHandler);
            processor.ProcessErrorAsync += errorHandler;

            await processor.StartProcessingAsync(cancellationToken);
        }
    }
}
