using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using laget.Azure.ServiceBus.Constants;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace laget.Azure.ServiceBus.Topic
{
    public interface ITopicReceiver
    {
        void Register(Func<ServiceBusMessage, CancellationToken, Task> messageHandler, Func<ProcessErrorEventArgs, Task> exceptionHandler);
    }

    public class TopicReceiver : ITopicReceiver
    {
        private const int DefaultMaxConcurrentCalls = 10;
        private const bool DefaultAutoComplete = true;

        private readonly BlobContainerClient _blobContainerClient;
        private readonly string _connectionString;
        private readonly TopicOptions _topicOptions;

        public TopicReceiver(string connectionString, TopicOptions topicOptions)
            : this(connectionString, topicOptions, null)
        { }

        public TopicReceiver(string connectionString, TopicOptions topicOptions, string blobConnectionString, string blobContainer)
            : this(connectionString, topicOptions, new BlobContainerClient(blobConnectionString, blobContainer))
        { }

        public TopicReceiver(string connectionString, TopicOptions topicOptions, BlobContainerClient blobContainerClient)
        {
            _blobContainerClient = blobContainerClient;
            _blobContainerClient?.CreateIfNotExists();
            _connectionString = connectionString;
            _topicOptions = topicOptions;
        }


        public void Register(Func<ServiceBusReceivedMessage, CancellationToken, Task> messageHandler, Func<ProcessErrorEventArgs, Task> exceptionHandler)
        {
            Register(messageHandler, new MessageHandlerOptions(exceptionHandler) { MaxConcurrentCalls = DefaultMaxConcurrentCalls, AutoComplete = DefaultAutoComplete });
        }

        public void Register(Func<ServiceBusMessage, CancellationToken, Task> messageHandler, MessageHandlerOptions handlerOptions)
        {
            _client.RegisterMessageHandler(HandlerWrapper(messageHandler), handlerOptions);
        }

        private Func<ServiceBusMessage, CancellationToken, Task> HandlerWrapper(Func<ServiceBusMessage, CancellationToken, Task> callback)
        {
            return async (message, ct) =>
            {
                if (message.UserProperties.ContainsKey(MessageConstants.BlobIdHeader))
                {

                    if (_blobContainerClient == null)
                    {
                        throw new InvalidOperationException("Received message with blob payload but receiver is not configured to use blobs");
                    }

                    var blobId = message.UserProperties[MessageConstants.BlobIdHeader];
                    if (blobId is string blobName)
                    {
                        var blobClient = _blobContainerClient.GetBlobClient(BlobPath(blobName));
                        var response = await blobClient.DownloadContentAsync(ct);
                        if (response != null)
                        {
                            message.Body = response.Value.Content.ToArray();
                        }

                        await callback(message, ct);
                        await blobClient.DeleteAsync(cancellationToken: ct);
                    }
                }
                else
                {
                    await callback(message, ct);
                }
            };
        }

        private string BlobPath(string blobName) => $"{_topicOptions}/{blobName}";
    }
}
