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
        private readonly ServiceBusClient _serviceBusClient;
        private readonly QueueOptions _queueOptions;

        public QueueSender(string connectionString, QueueOptions queueOptions)
            : this(null, new ServiceBusClient(connectionString, queueOptions.ServiceBusClientOptions), queueOptions)
        { }

        public QueueSender(string blobConnectionString, string blobContainer, string connectionString, QueueOptions queueOptions)
            : this(new BlobContainerClient(blobConnectionString, blobContainer), new ServiceBusClient(connectionString, queueOptions.ServiceBusClientOptions), queueOptions)
        { }

        public QueueSender(BlobContainerClient blobContainerClient, ServiceBusClient serviceBusClient, QueueOptions queueOptions)
        {
            _blobContainerClient = blobContainerClient;
            _blobContainerClient?.CreateIfNotExists();
            _serviceBusClient = serviceBusClient;
            _queueOptions = queueOptions;
        }

        public async Task SendAsync(IMessage message)
        {
            var sender = _serviceBusClient.CreateSender(_queueOptions.QueueName);
            var msg = await message.ToServiceBusMessageAsync(_blobContainerClient, _queueOptions.QueueName);

            await sender.SendMessageAsync(msg);
            await sender.DisposeAsync();
        }

        public async Task ScheduleAsync(IMessage message, DateTimeOffset offset)
        {
            var sender = _serviceBusClient.CreateSender(_queueOptions.QueueName);
            var msg = await message.ToServiceBusMessageAsync(_blobContainerClient, _queueOptions.QueueName);

            await sender.ScheduleMessageAsync(msg, offset);
            await sender.DisposeAsync();
        }

        public async Task SendAsync(string json)
        {
            var sender = _serviceBusClient.CreateSender(_queueOptions.QueueName);
            var msg = await json.ToServiceBusMessageAsync(_blobContainerClient, _queueOptions.QueueName);

            await sender.SendMessageAsync(msg);
            await sender.DisposeAsync();
        }

        public async Task ScheduleAsync(string json, DateTimeOffset offset)
        {
            var sender = _serviceBusClient.CreateSender(_queueOptions.QueueName);
            var msg = await json.ToServiceBusMessageAsync(_blobContainerClient, _queueOptions.QueueName);

            await sender.ScheduleMessageAsync(msg, offset);
            await sender.DisposeAsync();
        }

        public async Task Deschedule(long sequenceNumber)
        {
            var sender = _serviceBusClient.CreateSender(_queueOptions.QueueName);

            await sender.CancelScheduledMessageAsync(sequenceNumber);
            await sender.DisposeAsync();
        }
    }
}
