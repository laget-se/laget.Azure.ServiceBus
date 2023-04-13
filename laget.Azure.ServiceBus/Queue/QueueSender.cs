using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace laget.Azure.ServiceBus.Queue
{
    public interface IQueueSender
    {
        Task SendAsync(IMessage message);
        Task ScheduleAsync(IMessage message, DateTimeOffset offset);
        Task SendAsync(string json);
        Task ScheduleAsync(string json, DateTimeOffset offset);
        Task Deschedule(long sequenceNumber);

        void RegisterPlugin(ServiceBusPlugin plugin);
        void UnregisterPlugin(ServiceBusPlugin plugin);
        IEnumerable<ServiceBusPlugin> RegisteredPlugins();
    }

    public class QueueSender : IQueueSender
    {
        private readonly IQueueClient _client;

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

        public async Task Deschedule(long sequenceNumber)
        {
            await _client.CancelScheduledMessageAsync(sequenceNumber);
        }

        public void RegisterPlugin(ServiceBusPlugin plugin)
        {
            _client.RegisterPlugin(plugin);
        }

        public void UnregisterPlugin(ServiceBusPlugin plugin)
        {
            _client.UnregisterPlugin(plugin.Name);
        }

        public IEnumerable<ServiceBusPlugin> RegisteredPlugins()
        {
            return _client.RegisteredPlugins;
        }
    }
}
