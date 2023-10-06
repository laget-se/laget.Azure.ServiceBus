using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using laget.Azure.ServiceBus.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        Task Deschedule(long sequenceNumber);
    }

    public class TopicSender : ITopicSender
    {
        private readonly BlobContainerClient _blobContainerClient;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly TopicOptions _topicOptions;

        public TopicSender(string connectionString, TopicOptions topicOptions)
            : this(null, new ServiceBusClient(connectionString, topicOptions.ServiceBusClientOptions), topicOptions)
        { }

        public TopicSender(string blobConnectionString, string blobContainer, string connectionString, TopicOptions topicOptions)
            : this(new BlobContainerClient(blobConnectionString, blobContainer), new ServiceBusClient(connectionString, topicOptions.ServiceBusClientOptions), topicOptions)
        { }

        public TopicSender(BlobContainerClient blobContainerClient, ServiceBusClient serviceBusClient, TopicOptions topicOptions)
        {
            _blobContainerClient = blobContainerClient;
            _blobContainerClient?.CreateIfNotExists();
            _serviceBusClient = serviceBusClient;
            _topicOptions = topicOptions;
        }

        public async Task SendAsync(IMessage message)
        {
            var sender = _serviceBusClient.CreateSender(_topicOptions.TopicName);
            var msg = await message.ToServiceBusMessageAsync(_blobContainerClient, _topicOptions.TopicName);

            await sender.SendMessageAsync(msg);
            await sender.DisposeAsync();
        }

        public async Task SendAsync(IEnumerable<IMessage> messages)
        {
            var queue = new Queue<ServiceBusMessage>();
            foreach (var message in messages)
            {
                queue.Enqueue(await message.ToServiceBusMessageAsync(_blobContainerClient, _topicOptions.TopicName));
            }

            var sender = _serviceBusClient.CreateSender(_topicOptions.TopicName);

            await sender.SendMessagesAsync(queue);
            await sender.DisposeAsync();
        }

        public async Task ScheduleAsync(IMessage message, DateTimeOffset offset)
        {
            var sender = _serviceBusClient.CreateSender(_topicOptions.TopicName);
            var msg = await message.ToServiceBusMessageAsync(_blobContainerClient, _topicOptions.TopicName);

            await sender.ScheduleMessageAsync(msg, offset);
            await sender.DisposeAsync();
        }

        public async Task SendAsync(string json)
        {
            var sender = _serviceBusClient.CreateSender(_topicOptions.TopicName);
            var msg = await json.ToServiceBusMessageAsync(_blobContainerClient, _topicOptions.TopicName);

            await sender.SendMessageAsync(msg);
            await sender.DisposeAsync();
        }

        public async Task SendAsync(IEnumerable<string> messages)
        {
            var queue = new Queue<ServiceBusMessage>();
            foreach (var message in messages)
            {
                queue.Enqueue(await message.ToServiceBusMessageAsync(_blobContainerClient, _topicOptions.TopicName));
            }

            var sender = _serviceBusClient.CreateSender(_topicOptions.TopicName);

            await sender.SendMessagesAsync(queue);
            await sender.DisposeAsync();
        }

        public async Task ScheduleAsync(string json, DateTimeOffset offset)
        {
            var sender = _serviceBusClient.CreateSender(_topicOptions.TopicName);
            var msg = await json.ToServiceBusMessageAsync(_blobContainerClient, _topicOptions.TopicName);

            await sender.ScheduleMessageAsync(msg, offset);
            await sender.DisposeAsync();
        }

        public async Task Deschedule(long sequenceNumber)
        {
            var sender = _serviceBusClient.CreateSender(_topicOptions.TopicName);

            await sender.CancelScheduledMessageAsync(sequenceNumber);
            await sender.DisposeAsync();
        }
    }
}
