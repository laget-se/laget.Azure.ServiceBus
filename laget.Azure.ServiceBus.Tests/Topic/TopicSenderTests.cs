using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using laget.Azure.ServiceBus.Topic;
using Microsoft.Azure.ServiceBus;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Xunit;

namespace laget.Azure.ServiceBus.Tests.Topic
{
    using Message = Microsoft.Azure.ServiceBus.Message;
    public class TopicSenderTests
    {
        [Fact]
        public void ShouldSendSmallContentInMessagePayload()
        {
            const string message = "{ \"key\": \"value\" }";
            var topicClient = new Mock<ITopicClient>();
            var blobContainerClient = new Mock<BlobContainerClient>();
            var sut = new TopicSender(topicClient.Object, blobContainerClient.Object);

            sut.SendAsync(message).Wait();

            blobContainerClient.Verify(b => b.CreateIfNotExists(It.IsAny<PublicAccessType>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<BlobContainerEncryptionScopeOptions>(), It.IsAny<CancellationToken>()));
            blobContainerClient.VerifyNoOtherCalls();
            topicClient.Verify(tc => tc.SendAsync(It.Is<Message>(m => Encoding.UTF8.GetString(m.Body) == message)));
            topicClient.VerifyNoOtherCalls();
        }

        [Fact]
        public void ShouldSendLargeContentUsingBlobStorage()
        {
            var blobId = "";
            var blobPath = "";
            var message = new string('a', 256 * 1024);
            var topicClient = new Mock<ITopicClient>();
            topicClient.Setup(tc => tc.TopicName).Returns("topic").Verifiable();
            var blobContainerClient = new Mock<BlobContainerClient>();
            blobContainerClient.Setup(bc =>
                bc.UploadBlobAsync(It.IsAny<string>(), It.IsAny<BinaryData>(), It.IsAny<CancellationToken>())).Callback(
                (string s, BinaryData _, CancellationToken __) =>
                {
                    blobPath = s;
                    blobId = s.Substring("topic/".Length);
                });
            var sut = new TopicSender(topicClient.Object, blobContainerClient.Object);

            sut.SendAsync(message).Wait();

            blobContainerClient.Verify(b => b.CreateIfNotExists(It.IsAny<PublicAccessType>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<BlobContainerEncryptionScopeOptions>(), It.IsAny<CancellationToken>()));
            blobContainerClient.Verify(bc =>
                bc.UploadBlobAsync(It.Is<string>(s => s == blobPath), It.Is<BinaryData>(bd => bd.ToString() == message), It.IsAny<CancellationToken>()));
            blobContainerClient.VerifyNoOtherCalls();

            topicClient.Verify();
            topicClient.Verify(tc => tc.SendAsync(It.Is<Message>(m => m.Body == null && (string)m.UserProperties[TopicConstants.BlobIdHeader] == blobId)));
            topicClient.VerifyNoOtherCalls();
        }

        [Fact]
        public void ShouldThrowExceptionIfSendingTooLargeMsgWithoutBlobStorage()
        {
            var message = new string('a', 256 * 1024);
            var topicClient = new Mock<ITopicClient>();
            var sut = new TopicSender(topicClient.Object, null);


            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await sut.SendAsync(message);
            });
        }
    }
}