using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;

namespace laget.Azure.ServiceBus.Extensions
{
    public static class ProcessMessageEventArgsExtensions
    {
        public static TEntity Deserialize<TEntity>(this ProcessMessageEventArgs args) where TEntity : Message
        {
            return args.Message.Deserialize<TEntity>();
        }

        public static TEntity Deserialize<TEntity>(this ProcessMessageEventArgs args, BlobClient blobClient) where TEntity : Message
        {
            return args.Message.Deserialize<TEntity>(blobClient);
        }
    }
}
