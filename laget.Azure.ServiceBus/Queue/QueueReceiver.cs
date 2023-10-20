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
        Task StopAsync(CancellationToken cancellationToken = default);
        Task DisposeAsync();
    }

    public class QueueReceiver : IQueueReceiver
    {
        private readonly QueueOptions _queueQueueOptions;

        internal static BlobContainerClient BlobContainerClient;
        internal static ServiceBusClient ServiceBusClient;
        internal static ServiceBusProcessor ServiceBusProcessor;

        public QueueReceiver(string connectionString, QueueOptions queueOptions)
            : this(null, new ServiceBusClient(connectionString, queueOptions.ServiceBusClientOptions), queueOptions)
        { }

        public QueueReceiver(string blobConnectionString, string blobContainer, string connectionString, QueueOptions queueOptions)
            : this(new BlobContainerClient(blobConnectionString, blobContainer), new ServiceBusClient(connectionString, queueOptions.ServiceBusClientOptions), queueOptions)
        { }

        internal QueueReceiver(BlobContainerClient blobContainerClient, ServiceBusClient serviceBusClient, QueueOptions queueOptions)
        {
            BlobContainerClient = blobContainerClient;
            ServiceBusClient = serviceBusClient;
            _queueQueueOptions = queueOptions;
        }

        public async Task RegisterAsync(Func<ProcessMessageEventArgs, ServiceBusMessage, Task> messageHandler, Func<ProcessErrorEventArgs, Task> errorHandler, CancellationToken cancellationToken = default)
        {
            ServiceBusProcessor = ServiceBusClient.CreateProcessor(_queueQueueOptions.QueueName, _queueQueueOptions.ServiceBusProcessorOptions);

            ServiceBusProcessor.ProcessMessageAsync += new MessageHandlerWrapper(BlobContainerClient, _queueQueueOptions.QueueName).Handler(messageHandler);
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
