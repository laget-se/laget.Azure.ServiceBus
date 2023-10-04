using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using laget.Azure.ServiceBus.Constants;
using Newtonsoft.Json;
using System;

namespace laget.Azure.ServiceBus.Extensions
{
    public static class ServiceBusReceivedMessageExtensions
    {
        public static TEntity Deserialize<TEntity>(this ServiceBusReceivedMessage message, BlobClient blobClient) where TEntity : Message
        {
            if (blobClient != null && message.ApplicationProperties.TryGetValue(MessageConstants.BlobIdHeader, out var blobId))
            {
                var response = blobClient.DownloadContent();
                if (response.HasValue)
                {
                    var entity = JsonConvert.DeserializeObject<TEntity>(response.Value.Content.ToString(),
                        new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.Auto
                        });

                    entity.Id = message.MessageId;

                    return entity;
                }

                return null;
            }
            else
            {
                var entity = JsonConvert.DeserializeObject<TEntity>(message.Body.ToString(),
                    new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Auto
                    });

                entity.Id = message.MessageId;

                return entity;
            }
        }

        public static TEntity Deserialize<TEntity>(this ServiceBusReceivedMessage message) where TEntity : Message
        {
            var entity = JsonConvert.DeserializeObject<TEntity>(message.Body.ToString(),
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                });

            entity.Id = message.MessageId;

            return entity;
        }

        public static DateTime ExpiresAt(this ServiceBusReceivedMessage message)
        {
            return (DateTime.UtcNow + message.TimeToLive);
        }

        public static DateTime ExpiresAtUtc(this ServiceBusReceivedMessage message)
        {
            return (DateTime.UtcNow + message.TimeToLive).ToLocalTime();
        }

        public static DateTime ScheduledAt(this ServiceBusReceivedMessage message)
        {
            return message.ScheduledEnqueueTime.LocalDateTime;
        }

        public static DateTime ScheduledAtUtc(this ServiceBusReceivedMessage message)
        {
            return message.ScheduledEnqueueTime.DateTime;
        }
    }
}
