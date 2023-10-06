using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;

namespace laget.Azure.ServiceBus.Extensions
{
    public static class ServiceBusMessageExtensions
    {
        public static TEntity Deserialize<TEntity>(this ServiceBusMessage message) where TEntity : Message
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
}
