﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using laget.Azure.ServiceBus.Extensions;

namespace laget.Azure.ServiceBus.Topic
{
    public interface ITopicSender
    {
        Task SendAsync(IMessage message);
        Task SendAsync(IEnumerable<IMessage> messages);
        Task ScheduleAsync(IMessage message, DateTimeOffset offset);
        Task SendAsync(string json);
        Task SendAsync(IEnumerable<string> messages);
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
            await _client.SendAsync(message.ToServiceBusMessage());
        }

        public async Task SendAsync(IEnumerable<IMessage> messages)
        {
            var sendList = new List<Microsoft.Azure.ServiceBus.Message>();

            foreach (var message in messages)
                sendList.Add(message.ToServiceBusMessage());

            await _client.SendAsync(sendList);
        }

        public async Task ScheduleAsync(IMessage message, DateTimeOffset offset)
        {
            await _client.ScheduleMessageAsync(message.ToServiceBusMessage(), offset);
        }

        public async Task SendAsync(string json)
        {
            var bytes = Encoding.UTF8.GetBytes(json);

            await _client.SendAsync(new Microsoft.Azure.ServiceBus.Message(bytes));
        }

        public async Task SendAsync(IEnumerable<string> messages)
        {
            var sendList = new List<Microsoft.Azure.ServiceBus.Message>();

            foreach (var message in messages)
                sendList.Add(new Microsoft.Azure.ServiceBus.Message(Encoding.UTF8.GetBytes(message)));

            await _client.SendAsync(sendList);
        }

        public async Task ScheduleAsync(string json, DateTimeOffset offset)
        {
            var bytes = Encoding.UTF8.GetBytes(json);

            await _client.ScheduleMessageAsync(new Microsoft.Azure.ServiceBus.Message(bytes), offset);
        }
    }
}
