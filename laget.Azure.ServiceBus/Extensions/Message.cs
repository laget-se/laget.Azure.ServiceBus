using System.Text;
using Newtonsoft.Json;

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
    }
}
