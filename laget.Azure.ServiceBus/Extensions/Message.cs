using Newtonsoft.Json;
using System;
using System.Text;

namespace laget.Azure.ServiceBus.Extensions
{
    public static class MessageExtension
    {
        public static TEntity Deserialize<TEntity>(this Microsoft.Azure.ServiceBus.Message message) where TEntity : Message
        {
            var entity = JsonConvert.DeserializeObject<TEntity>(Encoding.UTF8.GetString(message.Body),
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                });

            entity.Id = message.MessageId;
            entity.Source = message;

            return entity;
        }

        public static DateTime ScheduledAt(this Microsoft.Azure.ServiceBus.Message message)
        {
            return (message.ExpiresAtUtc - message.TimeToLive).ToLocalTime();
        }

        public static DateTime ScheduledAtUtc(this Microsoft.Azure.ServiceBus.Message message)
        {
            return (message.ExpiresAtUtc - message.TimeToLive);
        }

        public static byte[] GetBytes(this IMessage message)
        {
            var json = message.Serialize();
            return Encoding.UTF8.GetBytes(json);
        }
    }
}
