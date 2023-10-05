using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using laget.Azure.ServiceBus.Constants;
using laget.Azure.ServiceBus.Extensions;
using System;
using System.Threading.Tasks;

namespace laget.Azure.ServiceBus.Wrappers
{
    internal class MessageHandlerWrapper
    {
        private readonly BlobContainerClient _blobContainerClient;
        private readonly string _queueOrTopicName;

        internal MessageHandlerWrapper(BlobContainerClient blobContainerClient, string queueOrTopicName)
        {
            _blobContainerClient = blobContainerClient;
            _blobContainerClient?.CreateIfNotExists();
            _queueOrTopicName = queueOrTopicName;
        }

        public Func<ProcessMessageEventArgs, Task> Handler(Func<ProcessMessageEventArgs, ServiceBusMessage, Task> callback)
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
                        var client = _blobContainerClient.GetBlobClient(BlobPath(blobName));
                        var response = await client.DownloadContentAsync();

                        var message = new ServiceBusMessage();
                        if (response.HasValue)
                        {
                            message.Body = new BinaryData(response.Value.Content.ToArray());
                        }
                        await callback(args, message);
                    }
                }
                else
                {
                    await callback(args, args.Message.ToServiceBudMessage());
                }
            };
        }
        private string BlobPath(string blobName) => $"{_queueOrTopicName}/{blobName}";
    }
}
