using Azure.Storage.Blobs;
using laget.Azure.ServiceBus.Extensions;
using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace laget.Azure.ServiceBus.Topic
{
    using Message = Microsoft.Azure.ServiceBus.Message;

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
        private readonly BlobContainerClient _blobContainerClient;

        public TopicSender(string connectionString, TopicOptions options) :
            this(new TopicClient(connectionString, options.TopicName, options.RetryPolicy), null)
        { }

        public TopicSender(string connectionString, TopicOptions options, string blobConnectionString, string blobContainer) :
            this(new TopicClient(connectionString, options.TopicName, options.RetryPolicy),
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
            var sendList = new List<Message>();

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
            var sendList = new List<Message>();

            foreach (var message in messages)
                sendList.Add(await CreateMessage(Encoding.UTF8.GetBytes(message)));

            await _client.SendAsync(sendList);
        }

        public async Task ScheduleAsync(string json, DateTimeOffset offset)
        {
            var bytes = Encoding.UTF8.GetBytes(json);

            await _client.ScheduleMessageAsync(await CreateMessage(bytes), offset);
        }

        private async Task<Message> CreateMessage(byte[] body)
        {
            if (body.Length > TopicConstants.MaxMessageSize)
            {
                if (_blobContainerClient == null)
                {
                    throw new ArgumentException($"The body is too large, maximum size is ${TopicConstants.MaxMessageSize / 1024}KB");
                }

                var blobName = Guid.NewGuid().ToString();
                await _blobContainerClient.UploadBlobAsync(BlobPath(blobName), new BinaryData(body));
                var message = new Message();
                message.UserProperties.Add(TopicConstants.BlobIdHeader, blobName);
                return message;
            }

            return new Message(body);
        }

        private async Task<Message> CreateMessage(IMessage message)
        {
            return await CreateMessage(message.GetBytes());
        }

        private string BlobPath(string blobName) => $"{_client.TopicName}/{blobName}";
    }
}
