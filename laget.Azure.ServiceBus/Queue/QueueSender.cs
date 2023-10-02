using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using laget.Azure.ServiceBus.Extensions;
using System;
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
    }

    public class QueueSender : IQueueSender
    {
        private readonly BlobContainerClient _blobContainerClient;
        private readonly string _connectionString;
        private readonly QueueOptions _queueOptions;

        public QueueSender(string connectionString, QueueOptions queueOptions)
            : this(connectionString, queueOptions, null)
        { }

        public QueueSender(string connectionString, QueueOptions queueOptions, string blobConnectionString, string blobContainer)
            : this(connectionString, queueOptions, new BlobContainerClient(blobConnectionString, blobContainer))
        { }

        public QueueSender(string connectionString, QueueOptions queueOptions, BlobContainerClient blobContainerClient)
        {
            _blobContainerClient = blobContainerClient;
            _blobContainerClient?.CreateIfNotExists();
            _connectionString = connectionString;
            _queueOptions = queueOptions;
        }

        public async Task SendAsync(IMessage message)
        {
            var client = new ServiceBusClient(_connectionString);

            var sender = client.CreateSender(_queueOptions.QueueName);
            var msg = await message.ToServiceBusMessageAsync(_blobContainerClient, _queueOptions.QueueName);

            await sender.SendMessageAsync(msg);
            await client.DisposeAsync();
        }

        public async Task ScheduleAsync(IMessage message, DateTimeOffset offset)
        {
            var client = new ServiceBusClient(_connectionString);

            var sender = client.CreateSender(_queueOptions.QueueName);
            var msg = await message.ToServiceBusMessageAsync(_blobContainerClient, _queueOptions.QueueName);

            await sender.ScheduleMessageAsync(msg, offset);
            await client.DisposeAsync();
        }

        public async Task SendAsync(string json)
        {
            var client = new ServiceBusClient(_connectionString);

            var sender = client.CreateSender(_queueOptions.QueueName);
            var msg = await json.ToServiceBusMessageAsync(_blobContainerClient, _queueOptions.QueueName);

            await sender.SendMessageAsync(msg);
            await client.DisposeAsync();
        }

        public async Task ScheduleAsync(string json, DateTimeOffset offset)
        {
            var client = new ServiceBusClient(_connectionString);

            var sender = client.CreateSender(_queueOptions.QueueName);
            var msg = await json.ToServiceBusMessageAsync(_blobContainerClient, _queueOptions.QueueName);

            await sender.ScheduleMessageAsync(msg, offset);
            await client.DisposeAsync();
        }

        public async Task Deschedule(long sequenceNumber)
        {
            var client = new ServiceBusClient(_connectionString);

            var sender = client.CreateSender(_queueOptions.QueueName);

            await sender.CancelScheduledMessageAsync(sequenceNumber);
            await client.DisposeAsync();
        }
    }
}
