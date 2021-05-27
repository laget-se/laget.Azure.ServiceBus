﻿using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;

namespace laget.Azure.ServiceBus.Topic
{
    public interface ITopicSender
    {
        Task SendAsync(IMessage message);
        Task ScheduleAsync(IMessage message, DateTimeOffset offset);
        Task SendAsync(string json);
        Task ScheduleAsync(string json, DateTimeOffset offset);
    }

    public class TopicSender : ITopicSender
    {
        private readonly ITopicClient _client;

        public TopicSender(string connectionString, TopicOptions options)
        {
            _client = new TopicClient(connectionString, options.TopicName, options.RetryPolicy);
        }


        public async Task SendAsync(IMessage message)
        {
            var json = message.Serialize();
            var bytes = Encoding.UTF8.GetBytes(json);

            await _client.SendAsync(new Microsoft.Azure.ServiceBus.Message(bytes));
        }

        public async Task ScheduleAsync(IMessage message, DateTimeOffset offset)
        {
            var json = message.Serialize();
            var bytes = Encoding.UTF8.GetBytes(json);

            await _client.ScheduleMessageAsync(new Microsoft.Azure.ServiceBus.Message(bytes), offset);
        }

        public async Task SendAsync(string json)
        {
            var bytes = Encoding.UTF8.GetBytes(json);

            await _client.SendAsync(new Microsoft.Azure.ServiceBus.Message(bytes));
        }

        public async Task ScheduleAsync(string json, DateTimeOffset offset)
        {
            var bytes = Encoding.UTF8.GetBytes(json);

            await _client.ScheduleMessageAsync(new Microsoft.Azure.ServiceBus.Message(bytes), offset);
        }
    }
}
