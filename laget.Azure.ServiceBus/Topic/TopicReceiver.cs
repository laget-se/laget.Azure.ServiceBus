using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using laget.Azure.ServiceBus.Constants;
using System;
using System.Threading.Tasks;

namespace laget.Azure.ServiceBus.Topic
{
    public interface ITopicReceiver
    {
        Task Register(Func<ProcessMessageEventArgs, Task> messageHandler, Func<ProcessErrorEventArgs, Task> errorHandler);
    }

    public class TopicReceiver : ITopicReceiver
    {
        private readonly BlobContainerClient _blobContainerClient;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly TopicOptions _topicOptions;

        public TopicReceiver(string connectionString, TopicOptions topicOptions)
            : this(null, new ServiceBusClient(connectionString, topicOptions.ServiceBusClientOptions), topicOptions)
        { }

        public TopicReceiver(string blobConnectionString, string blobContainer, string connectionString, TopicOptions topicOptions)
            : this(new BlobContainerClient(blobConnectionString, blobContainer), new ServiceBusClient(connectionString, topicOptions.ServiceBusClientOptions), topicOptions)
        { }

        public TopicReceiver(BlobContainerClient blobContainerClient, ServiceBusClient serviceBusClient, TopicOptions topicOptions)
        {
            _blobContainerClient = blobContainerClient;
            _blobContainerClient?.CreateIfNotExists();
            _serviceBusClient = serviceBusClient;
            _topicOptions = topicOptions;
        }

        public async Task Register(Func<ProcessMessageEventArgs, Task> messageHandler, Func<ProcessErrorEventArgs, Task> errorHandler)
        {
            var processor = _serviceBusClient.CreateProcessor(_topicOptions.TopicName, _topicOptions.SubscriptionName);

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

        private string BlobPath(string blobName) => $"{_topicOptions}/{blobName}";
    }
}
