using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using laget.Azure.ServiceBus.Extensions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace laget.Azure.ServiceBus.Topic
{
    public interface ITopicSender
    {
        Task SendAsync(IMessage message, CancellationToken cancellationToken = default);
        Task SendAsync(IEnumerable<IMessage> messages, CancellationToken cancellationToken = default);
        Task ScheduleAsync(IMessage message, DateTimeOffset offset, CancellationToken cancellationToken = default);
        Task SendAsync(string json, CancellationToken cancellationToken = default);
        Task SendAsync(IEnumerable<string> messages, CancellationToken cancellationToken = default);
        Task ScheduleAsync(string json, DateTimeOffset offset, CancellationToken cancellationToken = default);
        Task Deschedule(long sequenceNumber, CancellationToken cancellationToken = default);
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

        public async Task SendAsync(IMessage message, CancellationToken cancellationToken = default)
        {
            var sender = _serviceBusClient.CreateSender(_topicOptions.TopicName);
            var msg = await message.ToServiceBusMessageAsync(_blobContainerClient, _topicOptions.TopicName);

            await sender.SendMessageAsync(msg, cancellationToken);
            await sender.DisposeAsync();
        }

        public async Task SendAsync(IEnumerable<IMessage> messages, CancellationToken cancellationToken = default)
        {
            var queue = new Queue<ServiceBusMessage>();
            foreach (var message in messages)
            {
                queue.Enqueue(await message.ToServiceBusMessageAsync(_blobContainerClient, _topicOptions.TopicName));
            }

            var sender = _serviceBusClient.CreateSender(_topicOptions.TopicName);

            await sender.SendMessagesAsync(queue, cancellationToken);
            await sender.DisposeAsync();
        }

        public async Task ScheduleAsync(IMessage message, DateTimeOffset offset, CancellationToken cancellationToken = default)
        {
            var sender = _serviceBusClient.CreateSender(_topicOptions.TopicName);
            var msg = await message.ToServiceBusMessageAsync(_blobContainerClient, _topicOptions.TopicName);

            await sender.ScheduleMessageAsync(msg, offset, cancellationToken);
            await sender.DisposeAsync();
        }

        public async Task SendAsync(string json, CancellationToken cancellationToken = default)
        {
            var sender = _serviceBusClient.CreateSender(_topicOptions.TopicName);
            var msg = await json.ToServiceBusMessageAsync(_blobContainerClient, _topicOptions.TopicName);

            await sender.SendMessageAsync(msg, cancellationToken);
            await sender.DisposeAsync();
        }

        public async Task SendAsync(IEnumerable<string> messages, CancellationToken cancellationToken = default)
        {
            var queue = new Queue<ServiceBusMessage>();
            foreach (var message in messages)
            {
                queue.Enqueue(await message.ToServiceBusMessageAsync(_blobContainerClient, _topicOptions.TopicName));
            }

            var sender = _serviceBusClient.CreateSender(_topicOptions.TopicName);

            await sender.SendMessagesAsync(queue, cancellationToken);
            await sender.DisposeAsync();
        }

        public async Task ScheduleAsync(string json, DateTimeOffset offset, CancellationToken cancellationToken = default)
        {
            var sender = _serviceBusClient.CreateSender(_topicOptions.TopicName);
            var msg = await json.ToServiceBusMessageAsync(_blobContainerClient, _topicOptions.TopicName);

            await sender.ScheduleMessageAsync(msg, offset, cancellationToken);
            await sender.DisposeAsync();
        }

        public async Task Deschedule(long sequenceNumber, CancellationToken cancellationToken = default)
        {
            var sender = _serviceBusClient.CreateSender(_topicOptions.TopicName);

            await sender.CancelScheduledMessageAsync(sequenceNumber, cancellationToken);
            await sender.DisposeAsync();
        }
    }
}
