using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using laget.Azure.ServiceBus.Constants;
using System;
using System.Threading.Tasks;

namespace laget.Azure.ServiceBus.Queue
{
    public interface IQueueReceiver
    {
        Task Register(Func<ProcessMessageEventArgs, Task> messageHandler, Func<ProcessErrorEventArgs, Task> errorHandler);
        Task Register(Func<ProcessMessageEventArgs, Task> messageHandler, Func<ProcessErrorEventArgs, Task> errorHandler, ServiceBusClientOptions serviceBusClientOptions);
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

        public QueueReceiver(BlobContainerClient blobContainerClient, ServiceBusClient serviceBusClient, QueueOptions queueOptions)
        {
            _blobContainerClient = blobContainerClient;
            _blobContainerClient?.CreateIfNotExists();
            _serviceBusClient = serviceBusClient;
            _queueQueueOptions = queueOptions;
        }

        public async Task Register(Func<ProcessMessageEventArgs, Task> messageHandler, Func<ProcessErrorEventArgs, Task> errorHandler)
        {
            var processor = _serviceBusClient.CreateProcessor(_queueQueueOptions.QueueName);

            processor.ProcessMessageAsync += HandlerWrapper(messageHandler);
            processor.ProcessErrorAsync += errorHandler;

            await processor.StartProcessingAsync();
            await processor.DisposeAsync();
        }

        public async Task Register(Func<ProcessMessageEventArgs, Task> messageHandler, Func<ProcessErrorEventArgs, Task> errorHandler, ServiceBusClientOptions serviceBusClientOptions)
        {
            var processor = _serviceBusClient.CreateProcessor(_queueQueueOptions.QueueName);

            processor.ProcessMessageAsync += HandlerWrapper(messageHandler);
            processor.ProcessErrorAsync += errorHandler;

            await processor.StartProcessingAsync();
            await processor.DisposeAsync();
        }


        private Func<ProcessMessageEventArgs, Task> HandlerWrapper(Func<ProcessMessageEventArgs, Task> callback)
        {
            return async (args) =>
            {
                if (args.Message.ApplicationProperties.TryGetValue(MessageConstants.BlobIdHeader, out var blobId))
                {
                    if (_blobContainerClient == null)
                    {
                        throw new InvalidOperationException("Received message with blob payload but receiver is not configured to use blobs");
                    }

                    if (blobId is string blobName)
                    {
                        var blobClient = _blobContainerClient.GetBlobClient(BlobPath(blobName));
                        var response = await blobClient.DownloadContentAsync();
                        if (response != null)
                        {
                            //TODO: How do we solve this? Helper methods used in implementing projects?
                            //var message = new ServiceBusMessage(response.Value.Content);
                            //await callback(message);
                            await callback(args);
                        }

                        await callback(args);
                        await blobClient.DeleteAsync();
                    }
                }
                else
                {
                    await callback(args);
                }
            };
        }

        private string BlobPath(string blobName) => $"{_queueQueueOptions.QueueName}/{blobName}";
    }
}
