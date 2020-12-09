using System;
using Newtonsoft.Json;

namespace laget.Azure
{
    public interface IMessage
    {
        string Id { get; set; }
        string Type { get; set; }
        string Category { get; set; }
        DateTime CreatedAt { get; }
        Microsoft.Azure.ServiceBus.Message Source { get; set; }

        string Serialize();
    }

    [Serializable]
    public class Message : IMessage
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("category")]
        public string Category { get; set; }
        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [JsonProperty("source")]
        public Microsoft.Azure.ServiceBus.Message Source { get; set; }

        public string Serialize() => JsonConvert.SerializeObject(this);
    }
}
