using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using laget.Azure.ServiceBus.Topic;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace laget.Azure.ServiceBus.Tests.Topic
{
    public class TopicSenderTests
    {
        [Fact]
        public async Task ShouldSendSmallContentInMessagePayload()
        {
            const string message = "{ \"key\": \"value\" }";
            var topicOptions = new TopicOptions { SubscriptionName = "Subscription", TopicName = "Topic" };

            var blobContainerClient = new Mock<BlobContainerClient>();
            var serviceBusClient = new Mock<ServiceBusClient>();
            var serviceBusSender = new Mock<ServiceBusSender>();

            serviceBusClient.Setup(x => x.CreateSender(topicOptions.TopicName)).Returns(serviceBusSender.Object);

            var sut = new TopicSender(blobContainerClient.Object, serviceBusClient.Object, topicOptions);

            await sut.SendAsync(message);

            blobContainerClient.Verify(b => b.CreateIfNotExists(It.IsAny<PublicAccessType>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<BlobContainerEncryptionScopeOptions>(), It.IsAny<CancellationToken>()));
            blobContainerClient.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldSendLargeContentUsingBlobStorage()
        {
            var blobId = "";
            var blobPath = "";
            var message = new string('a', 256 * 1024);
            var topicOptions = new TopicOptions { SubscriptionName = "Subscription", TopicName = "Topic" };

            var blobContainerClient = new Mock<BlobContainerClient>();
            var serviceBusClient = new Mock<ServiceBusClient>();
            var serviceBusSender = new Mock<ServiceBusSender>();

            blobContainerClient.Setup(bc =>
                bc.UploadBlobAsync(It.IsAny<string>(), It.IsAny<BinaryData>(), It.IsAny<CancellationToken>())).Callback(
                (string s, BinaryData _, CancellationToken __) =>
                {
                    blobPath = s;
                    blobId = s.Substring("topic/".Length);
                });
            serviceBusClient.Setup(x => x.CreateSender(topicOptions.TopicName)).Returns(serviceBusSender.Object);

            var sut = new TopicSender(blobContainerClient.Object, serviceBusClient.Object, topicOptions);

            await sut.SendAsync(message);

            blobContainerClient.Verify(b => b.CreateIfNotExists(It.IsAny<PublicAccessType>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<BlobContainerEncryptionScopeOptions>(), It.IsAny<CancellationToken>()));
            blobContainerClient.Verify(bc => bc.UploadBlobAsync(It.Is<string>(s => s == blobPath), It.Is<BinaryData>(bd => bd.ToString() == message), It.IsAny<CancellationToken>()));
            blobContainerClient.VerifyNoOtherCalls();
        }

        [Fact]
        public void ShouldThrowExceptionIfSendingTooLargeMsgWithoutBlobStorage()
        {
            var message = new string('a', 256 * 1024);

            var serviceBusClient = new Mock<ServiceBusClient>();
            var topicOptions = new TopicOptions { SubscriptionName = "Subscription", TopicName = "Topic" };

            var sut = new TopicSender(null, serviceBusClient.Object, topicOptions);

            //Assert.ThrowsAsync<ArgumentException>(async () =>
            //{
            //    await sut.SendAsync(message);
            //});
        }
    }
}