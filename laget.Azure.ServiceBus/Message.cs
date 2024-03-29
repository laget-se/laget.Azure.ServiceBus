﻿using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace laget.Azure.ServiceBus
{
    public interface IMessage
    {
        string Id { get; set; }
        string Type { get; set; }
        string Category { get; set; }

        DateTime CreatedAt { get; }

        string Serialize();
    }

    [Serializable]
    public class Message : IMessage
    {
        [JsonProperty("id")]
        public virtual string Id { get; set; }
        [JsonProperty("type")]
        public virtual string Type { get; set; }
        [JsonProperty("category")]
        public virtual string Category { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual string Serialize() =>
            JsonConvert.SerializeObject(this, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented
            });
    }
}
