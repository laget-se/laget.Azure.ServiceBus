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
        private readonly string _connectionString;
        private readonly QueueOptions _queueQueueOptions;

        public QueueReceiver(string connectionString, QueueOptions queueOptions)
            : this(connectionString, queueOptions, null)
        { }

        public QueueReceiver(string connectionString, QueueOptions queueOptions, string blobConnectionString, string blobContainer)
            : this(connectionString, queueOptions, new BlobContainerClient(blobConnectionString, blobContainer))
        { }

        public QueueReceiver(string connectionString, QueueOptions queueOptions, BlobContainerClient blobContainerClient)
        {
            _blobContainerClient = blobContainerClient;
            _blobContainerClient?.CreateIfNotExists();
            _connectionString = connectionString;
            _queueQueueOptions = queueOptions;
        }

        public async Task Register(Func<ProcessMessageEventArgs, Task> messageHandler, Func<ProcessErrorEventArgs, Task> errorHandler)
        {
            var client = new ServiceBusClient(_connectionString);
            var processor = client.CreateProcessor(_queueQueueOptions.QueueName);

            processor.ProcessMessageAsync += HandlerWrapper(messageHandler);
            processor.ProcessErrorAsync += errorHandler;

            await processor.StartProcessingAsync();
            await client.DisposeAsync();
            await processor.DisposeAsync();
        }

        public async Task Register(Func<ProcessMessageEventArgs, Task> messageHandler, Func<ProcessErrorEventArgs, Task> errorHandler, ServiceBusClientOptions serviceBusClientOptions)
        {
            var client = new ServiceBusClient(_connectionString, serviceBusClientOptions);
            var processor = client.CreateProcessor(_queueQueueOptions.QueueName);

            processor.ProcessMessageAsync += HandlerWrapper(messageHandler);
            processor.ProcessErrorAsync += errorHandler;

            await processor.StartProcessingAsync();
            await client.DisposeAsync();
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
