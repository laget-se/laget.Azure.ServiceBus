using Azure.Storage.Blobs;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace laget.Azure.ServiceBus.Topic
{
    using Message = Microsoft.Azure.ServiceBus.Message;

    public interface ITopicReceiver
    {
        void Register(Func<Message, CancellationToken, Task> callback, Func<ExceptionReceivedEventArgs, Task> exceptionHandler, MessageHandlerOptions handlerOptions = null);
    }

    public class TopicReceiver : ITopicReceiver
    {
        private readonly IMessageReceiver _client;
        private readonly BlobContainerClient _blobContainerClient;
        private readonly string _topic;

        public TopicReceiver(string connectionString, TopicOptions options) :
            this(new MessageReceiver(connectionString, EntityNameHelper.FormatSubscriptionPath(options.TopicName, options.SubscriptionName), options.ReceiveMode, options.RetryPolicy),
                 options.TopicName,
                null)
        { }

        public TopicReceiver(string connectionString, TopicOptions options, string blobConnectionString, string blobContainer) :
            this(new MessageReceiver(connectionString, EntityNameHelper.FormatSubscriptionPath(options.TopicName, options.SubscriptionName), options.ReceiveMode, options.RetryPolicy),
                 options.TopicName,
                 new BlobContainerClient(blobConnectionString, blobContainer))
        { }

        public TopicReceiver(IMessageReceiver messageReceiver, string topic, BlobContainerClient blobContainerClient)
        {
            _client = messageReceiver;
            _topic = topic;
            _blobContainerClient = blobContainerClient;
            _blobContainerClient?.CreateIfNotExists();
        }


        public void Register(Func<Message, CancellationToken, Task> callback, Func<ExceptionReceivedEventArgs, Task> exceptionHandler, MessageHandlerOptions handlerOptions = null)
        {
            if (handlerOptions == null)
                handlerOptions = new MessageHandlerOptions(exceptionHandler) { MaxConcurrentCalls = 10, AutoComplete = true };

            _client.RegisterMessageHandler(HandlerWrapper(callback), handlerOptions);
        }

        private Func<Message, CancellationToken, Task> HandlerWrapper(Func<Message, CancellationToken, Task> callback)
        {
            return async (message, ct) =>
            {
                if (message.UserProperties.ContainsKey(TopicConstants.BlobIdHeader))
                {

                    if (_blobContainerClient == null)
                    {
                        throw new InvalidOperationException("Received message with blob payload but receiver is not configured to use blobs");
                    }
                    else
                    {
                        var blobId = message.UserProperties[TopicConstants.BlobIdHeader];
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
                }
                else
                {
                    await callback(message, ct);
                }
            };
        }

        private string BlobPath(string blobName) => $"{_topic}/{blobName}";
    }
}
