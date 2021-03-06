﻿using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;

namespace laget.Azure.ServiceBus.Queue
{
    public interface IQueueSender
    {
        Task SendAsync(IMessage message);
        Task ScheduleAsync(IMessage message, DateTimeOffset offset);
    }

    public class QueueSender : IQueueSender
    {
        readonly IQueueClient _client;

        public QueueSender(string connectionString, QueueOptions options)
        {
            _client = new QueueClient(connectionString, options.QueueName, options.ReceiveMode, options.RetryPolicy);
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
    }
}
