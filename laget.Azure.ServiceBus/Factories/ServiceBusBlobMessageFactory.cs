using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using laget.Azure.ServiceBus.Constants;
using System;
using System.Threading.Tasks;

namespace laget.Azure.ServiceBus.Factories
{
    internal class ServiceBusBlobMessageFactory
    {
        private readonly BlobContainerClient _blobContainerClient;
        private readonly string _queueOrTopicName;

        public ServiceBusBlobMessageFactory()
        {
        }

        public ServiceBusBlobMessageFactory(string blobConnectionString, string blobContainer, string queueOrTopicName)
            : this(new BlobContainerClient(blobConnectionString, blobContainer), queueOrTopicName)
        {
        }

        internal ServiceBusBlobMessageFactory(BlobContainerClient blobContainerClient, string queueOrTopicName)
        {
            _blobContainerClient = blobContainerClient;
            _blobContainerClient?.CreateIfNotExists();
            _queueOrTopicName = queueOrTopicName;
        }

        public Func<ProcessMessageEventArgs, Task> HandlerWrapper(Func<BlobClient, ProcessMessageEventArgs, Task> callback)
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
                        await callback(client, args);
                    }
                }
                else
                {
                    await callback(null, args);
                }
            };
        }
        private string BlobPath(string blobName) => $"{_queueOrTopicName}/{blobName}";
    }
}
