using System;
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
            await _client.SendAsync(message.ToServicebusMessage());
        }

        public async Task SendAsync(IList<IMessage> messageList)
        {
            var sendList = new List<Microsoft.Azure.ServiceBus.Message>();

            foreach (var message in messageList)
                sendList.Add(message.ToServicebusMessage());

            await _client.SendAsync(sendList);
        }

        public async Task ScheduleAsync(IMessage message, DateTimeOffset offset)
        {
            await _client.ScheduleMessageAsync(message.ToServicebusMessage(), offset);
        }

        public async Task SendAsync(string json)
        {
            var bytes = Encoding.UTF8.GetBytes(json);

            await _client.SendAsync(new Microsoft.Azure.ServiceBus.Message(bytes));
        }

        public async Task SendAsync(IList<string> messageList)
        {
            var sendList = new List<Microsoft.Azure.ServiceBus.Message>();

            foreach (var message in messageList)
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
