using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using laget.Azure.ServiceBus.Factories;
using laget.Azure.ServiceBus.Topic;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace laget.Azure.ServiceBus.Tests.Topic
{
    public class TopicReceiverTest
    {
        [Fact]
        public async Task ShouldGetPayloadFromMessageBodyIfNoHeaderPresent()
        {
            var body = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
            var message = new ServiceBusMessage(body);

            var blobContainerClient = new Mock<BlobContainerClient>();
            var blobFactory = new ServiceBusBlobMessageFactory(blobContainerClient.Object, "topic");
            var serviceBusClient = new Mock<ServiceBusClient>();
            var serviceBusSender = new Mock<ServiceBusSender>();
            var serviceBusProcessor = new Mock<ServiceBusProcessor>();
            var topicOptions = new TopicOptions { SubscriptionName = "Subscription", TopicName = "Topic" };

            Func<ProcessMessageEventArgs, Task> simulateMessageReceived = null;
            //serviceBusProcessor.Setup(x => x.ProcessMessageAsync += simulateMessageReceived);
            serviceBusClient.Setup(x => x.CreateProcessor(topicOptions.TopicName, topicOptions.SubscriptionName)).Returns(serviceBusProcessor.Object);

            //Func<ProcessMessageEventArgs, Task> simulateMessageReceived = null;
            //var messageReceiver = new Mock<IMessageReceiver>();
            //serviceBusSender
            //    .Setup(mr => mr.RegisterMessageHandler(It.IsAny<Func<ServiceBusMessage, CancellationToken, Task>>(), It.IsAny<MessageHandlerOptions>()))
            //    .Callback((Func<ServiceBusMessage, CancellationToken, Task> callback, MessageHandlerOptions _) =>
            //    {
            //        simulateMessageReceived = callback;
            //    })
            //    .Verifiable();

            var sut = new TopicReceiver(blobFactory, serviceBusClient.Object, topicOptions);

            ServiceBusReceivedMessage receivedMessage = null;
            await sut.RegisterAsync((m) =>
            {
                receivedMessage = m.Message;
                return Task.CompletedTask;
            }, _ => Task.CompletedTask);

            //messageReceiver.Verify();
            //Assert.NotNull(simulateMessageReceived);
            //simulateMessageReceived(message, new CancellationToken());

            //Assert.NotNull(receivedMessage);
            //Assert.Equal(body, receivedMessage.Body);

            //blobContainerClient.Verify(b => b.CreateIfNotExists(It.IsAny<PublicAccessType>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<BlobContainerEncryptionScopeOptions>(), It.IsAny<CancellationToken>()));
            //blobContainerClient.VerifyNoOtherCalls();
            //messageReceiver.VerifyNoOtherCalls();
        }

        //[Fact]
        //public void ShouldGetPayloadFromBlobStorageIfHeaderPresent()
        //{
        //    var body = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
        //    var message = new ServiceBusMessage();
        //    message.UserProperties.Add(MessageConstants.BlobIdHeader, $"topic/{Guid.Empty}");

        //    Func<Microsoft.Azure.ServiceBus.Message, CancellationToken, Task> simulateMessageReceived = null;
        //    var messageReceiver = new Mock<IMessageReceiver>();
        //    messageReceiver
        //        .Setup(mr => mr.RegisterMessageHandler(It.IsAny<Func<Microsoft.Azure.ServiceBus.Message, CancellationToken, Task>>(), It.IsAny<MessageHandlerOptions>()))
        //        .Callback((Func<Microsoft.Azure.ServiceBus.Message, CancellationToken, Task> callback, MessageHandlerOptions _) =>
        //        {
        //            simulateMessageReceived = callback;
        //        })
        //        .Verifiable();

        //    var blobResponse = new Mock<Response<BlobDownloadResult>>();
        //    blobResponse
        //        .Setup(br => br.Value)
        //        .Returns(BlobsModelFactory.BlobDownloadResult(new BinaryData(body)))
        //        .Verifiable();

        //    var blobClient = new Mock<BlobClient>();
        //    blobClient
        //        .Setup(bc => bc.DownloadContentAsync(It.IsAny<CancellationToken>()))
        //        .Returns(Task.FromResult(blobResponse.Object))
        //        .Verifiable();

        //    var blobContainerClient = new Mock<BlobContainerClient>();
        //    blobContainerClient
        //        .Setup(bc => bc.GetBlobClient(It.IsAny<string>()))
        //        .Returns(blobClient.Object)
        //        .Verifiable();

        //    var sut = new TopicReceiver(messageReceiver.Object, "topic", blobContainerClient.Object);

        //    Microsoft.Azure.ServiceBus.Message receivedMessage = null;
        //    sut.Register((m, _) =>
        //    {
        //        receivedMessage = m;
        //        return Task.CompletedTask;
        //    }, _ => Task.CompletedTask);

        //    messageReceiver.Verify();
        //    Assert.NotNull(simulateMessageReceived);
        //    simulateMessageReceived(message, new CancellationToken());

        //    Assert.NotNull(receivedMessage);
        //    Assert.Equal(body, receivedMessage.Body);

        //    blobResponse.Verify();
        //    blobResponse.VerifyNoOtherCalls();
        //    blobClient.Verify();
        //    blobClient.Verify(bc => bc.DeleteAsync(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()));
        //    blobClient.VerifyNoOtherCalls();
        //    blobContainerClient.Verify();
        //    blobContainerClient.Verify(b => b.CreateIfNotExists(It.IsAny<PublicAccessType>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<BlobContainerEncryptionScopeOptions>(), It.IsAny<CancellationToken>()));
        //    blobContainerClient.VerifyNoOtherCalls();
        //    messageReceiver.VerifyNoOtherCalls();
        //}

        //[Fact]
        //public void ShouldThrowExceptionWhenReceivingMessageWithBlobHeaderWithoutBlobStorage()
        //{
        //    var message = new Microsoft.Azure.ServiceBus.Message();
        //    message.UserProperties.Add(MessageConstants.BlobIdHeader, $"topic/{Guid.Empty}");

        //    Func<Microsoft.Azure.ServiceBus.Message, CancellationToken, Task> simulateMessageReceived = null;
        //    var messageReceiver = new Mock<IMessageReceiver>();
        //    messageReceiver
        //        .Setup(mr => mr.RegisterMessageHandler(It.IsAny<Func<Microsoft.Azure.ServiceBus.Message, CancellationToken, Task>>(), It.IsAny<MessageHandlerOptions>()))
        //        .Callback((Func<Microsoft.Azure.ServiceBus.Message, CancellationToken, Task> callback, MessageHandlerOptions _) =>
        //        {
        //            simulateMessageReceived = callback;
        //        })
        //        .Verifiable();

        //    var sut = new TopicReceiver(messageReceiver.Object, "topic", null);

        //    sut.Register((m, _) => Task.CompletedTask, _ => Task.CompletedTask);

        //    messageReceiver.Verify();
        //    Assert.NotNull(simulateMessageReceived);
        //    Assert.ThrowsAsync<InvalidOperationException>(async () =>
        //    {
        //        await simulateMessageReceived(message, new CancellationToken());
        //    });
        //}

        //[Fact]
        //public void ShouldNotDeleteBlobIfHandlerThrowsException()
        //{
        //    var body = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
        //    var message = new Microsoft.Azure.ServiceBus.Message();
        //    message.UserProperties.Add(MessageConstants.BlobIdHeader, $"topic/{Guid.Empty}");

        //    Func<Microsoft.Azure.ServiceBus.Message, CancellationToken, Task> simulateMessageReceived = null;
        //    var messageReceiver = new Mock<IMessageReceiver>();
        //    messageReceiver
        //        .Setup(mr => mr.RegisterMessageHandler(It.IsAny<Func<Microsoft.Azure.ServiceBus.Message, CancellationToken, Task>>(), It.IsAny<MessageHandlerOptions>()))
        //        .Callback((Func<Microsoft.Azure.ServiceBus.Message, CancellationToken, Task> callback, MessageHandlerOptions _) =>
        //        {
        //            simulateMessageReceived = callback;
        //        })
        //        .Verifiable();

        //    var blobResponse = new Mock<Response<BlobDownloadResult>>();
        //    blobResponse
        //        .Setup(br => br.Value)
        //        .Returns(BlobsModelFactory.BlobDownloadResult(new BinaryData(body)))
        //        .Verifiable();

        //    var blobClient = new Mock<BlobClient>();
        //    blobClient
        //        .Setup(bc => bc.DownloadContentAsync(It.IsAny<CancellationToken>()))
        //        .Returns(Task.FromResult(blobResponse.Object))
        //        .Verifiable();

        //    var blobContainerClient = new Mock<BlobContainerClient>();
        //    blobContainerClient
        //        .Setup(bc => bc.GetBlobClient(It.IsAny<string>()))
        //        .Returns(blobClient.Object)
        //        .Verifiable();

        //    var sut = new TopicReceiver(messageReceiver.Object, "topic", blobContainerClient.Object);

        //    sut.Register((m, _) => throw new Exception(), _ => Task.CompletedTask);

        //    messageReceiver.Verify();
        //    Assert.NotNull(simulateMessageReceived);

        //    simulateMessageReceived(message, new CancellationToken());

        //    blobResponse.Verify();
        //    blobResponse.VerifyNoOtherCalls();
        //    blobClient.Verify();
        //    blobClient.VerifyNoOtherCalls();
        //    blobContainerClient.Verify();
        //    blobContainerClient.Verify(b => b.CreateIfNotExists(It.IsAny<PublicAccessType>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<BlobContainerEncryptionScopeOptions>(), It.IsAny<CancellationToken>()));
        //    blobContainerClient.VerifyNoOtherCalls();
        //    messageReceiver.VerifyNoOtherCalls();
        //}
    }
}