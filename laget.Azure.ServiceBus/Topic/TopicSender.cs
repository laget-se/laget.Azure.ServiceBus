using Azure.Storage.Blobs;
using laget.Azure.ServiceBus.Extensions;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using System;
using System.Collections.Generic;
using System.Text;
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

        void RegisterPlugin(ServiceBusPlugin plugin);
        void UnregisterPlugin(ServiceBusPlugin plugin);
        IEnumerable<ServiceBusPlugin> RegisteredPlugins();
    }

    public class TopicSender : ITopicSender
    {
        private readonly ITopicClient _client;
        private readonly BlobContainerClient _blobContainerClient;

        public TopicSender(string connectionString, TopicOptions options)
            : this(new TopicClient(connectionString, options.TopicName, options.RetryPolicy), null)
        { }

        public TopicSender(string connectionString, TopicOptions options, string blobConnectionString, string blobContainer)
            : this(new TopicClient(connectionString, options.TopicName, options.RetryPolicy),
                 new BlobContainerClient(blobConnectionString, blobContainer))
        { }

        public TopicSender(ITopicClient topicClient, BlobContainerClient blobContainerClient)
        {
            _client = topicClient;
            _blobContainerClient = blobContainerClient;
            _blobContainerClient?.CreateIfNotExists();
        }

        public async Task SendAsync(IMessage message)
        {
            await _client.SendAsync(await CreateMessage(message));
        }

        public async Task SendAsync(IEnumerable<IMessage> messages)
        {
            var sendList = new List<Microsoft.Azure.ServiceBus.Message>();

            foreach (var message in messages)
                sendList.Add(await CreateMessage(message));

            await _client.SendAsync(sendList);
        }

        public async Task ScheduleAsync(IMessage message, DateTimeOffset offset)
        {
            await _client.ScheduleMessageAsync(await CreateMessage(message), offset);
        }

        public async Task SendAsync(string json)
        {
            var bytes = Encoding.UTF8.GetBytes(json);

            var msg = await CreateMessage(bytes);
            await _client.SendAsync(msg);
        }

        public async Task SendAsync(IEnumerable<string> messages)
        {
            var sendList = new List<Microsoft.Azure.ServiceBus.Message>();

            foreach (var message in messages)
                sendList.Add(await CreateMessage(Encoding.UTF8.GetBytes(message)));

            await _client.SendAsync(sendList);
        }

        public async Task ScheduleAsync(string json, DateTimeOffset offset)
        {
            var bytes = Encoding.UTF8.GetBytes(json);

            await _client.ScheduleMessageAsync(await CreateMessage(bytes), offset);
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

        private async Task<Microsoft.Azure.ServiceBus.Message> CreateMessage(byte[] body)
        {
            if (body.Length > TopicConstants.MaxMessageSize)
            {
                if (_blobContainerClient == null)
                {
                    throw new ArgumentException($"The body is too large, maximum size is ${TopicConstants.MaxMessageSize / 1024}KB");
                }

                var blobName = Guid.NewGuid().ToString();
                await _blobContainerClient.UploadBlobAsync(BlobPath(blobName), new BinaryData(body));
                var message = new Microsoft.Azure.ServiceBus.Message();
                message.UserProperties.Add(TopicConstants.BlobIdHeader, blobName);
                return message;
            }

            return new Microsoft.Azure.ServiceBus.Message(body);
        }

        private async Task<Microsoft.Azure.ServiceBus.Message> CreateMessage(IMessage message)
        {
            return await CreateMessage(message.GetBytes());
        }

        private string BlobPath(string blobName) => $"{_client.TopicName}/{blobName}";
    }
}
