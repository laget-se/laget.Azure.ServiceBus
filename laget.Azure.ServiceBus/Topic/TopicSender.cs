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
        private readonly string _connectionString;
        private readonly TopicOptions _topicOptions;

        public TopicSender(string connectionString, TopicOptions topicOptions)
            : this(connectionString, topicOptions, null)
        { }

        public TopicSender(string connectionString, TopicOptions topicOptions, string blobConnectionString, string blobContainer)
            : this(connectionString, topicOptions, new BlobContainerClient(blobConnectionString, blobContainer))
        { }

        public TopicSender(string connectionString, TopicOptions topicOptions, BlobContainerClient blobContainerClient)
        {
            _blobContainerClient = blobContainerClient;
            _blobContainerClient?.CreateIfNotExists();
            _connectionString = connectionString;
            _topicOptions = topicOptions;
        }

        public async Task SendAsync(IMessage message)
        {
            var client = new ServiceBusClient(_connectionString);

            var sender = client.CreateSender(_topicOptions.TopicName);
            var msg = await message.ToServiceBusMessageAsync(_blobContainerClient, _topicOptions.TopicName);

            await sender.SendMessageAsync(msg);
            await client.DisposeAsync();
        }

        public async Task SendAsync(IEnumerable<IMessage> messages)
        {
            var client = new ServiceBusClient(_connectionString);

            var queue = new Queue<ServiceBusMessage>();
            foreach (var message in messages)
            {
                queue.Enqueue(await message.ToServiceBusMessageAsync(_blobContainerClient, _topicOptions.TopicName));
            }

            var sender = client.CreateSender(_topicOptions.TopicName);

            await sender.SendMessagesAsync(queue);
            await client.DisposeAsync();
        }

        public async Task ScheduleAsync(IMessage message, DateTimeOffset offset)
        {
            var client = new ServiceBusClient(_connectionString);

            var sender = client.CreateSender(_topicOptions.TopicName);
            var msg = await message.ToServiceBusMessageAsync(_blobContainerClient, _topicOptions.TopicName);

            await sender.ScheduleMessageAsync(msg, offset);
            await client.DisposeAsync();
        }

        public async Task SendAsync(string json)
        {
            var client = new ServiceBusClient(_connectionString);

            var sender = client.CreateSender(_topicOptions.TopicName);
            var msg = await json.ToServiceBusMessageAsync(_blobContainerClient, _topicOptions.TopicName);

            await sender.SendMessageAsync(msg);
            await client.DisposeAsync();
        }

        public async Task SendAsync(IEnumerable<string> messages)
        {
            var client = new ServiceBusClient(_connectionString);

            var queue = new Queue<ServiceBusMessage>();
            foreach (var message in messages)
            {
                queue.Enqueue(await message.ToServiceBusMessageAsync(_blobContainerClient, _topicOptions.TopicName));
            }

            var sender = client.CreateSender(_topicOptions.TopicName);

            await sender.SendMessagesAsync(queue);
            await client.DisposeAsync();
        }

        public async Task ScheduleAsync(string json, DateTimeOffset offset)
        {
            var client = new ServiceBusClient(_connectionString);

            var sender = client.CreateSender(_topicOptions.TopicName);
            var msg = await json.ToServiceBusMessageAsync(_blobContainerClient, _topicOptions.TopicName);

            await sender.ScheduleMessageAsync(msg, offset);
            await client.DisposeAsync();
        }

        public async Task Deschedule(long sequenceNumber)
        {
            var client = new ServiceBusClient(_connectionString);

            var sender = client.CreateSender(_topicOptions.TopicName);

            await sender.CancelScheduledMessageAsync(sequenceNumber);
            await client.DisposeAsync();
        }
    }
}
