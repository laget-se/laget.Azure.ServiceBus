using Azure.Messaging.ServiceBus;
using laget.Azure.ServiceBus.Extensions;
using System;
using Xunit;

namespace laget.Azure.ServiceBus.Tests.Extensions
{
    public class MessageTests
    {
        [Fact]
        public void ShouldDeserializeMessage()
        {
            var body = new BinaryData("{ \"Name\": \"Jane Doe\" }".ToBytes());
            var serviceBusMessage = new ServiceBusMessage(body);
            var model = serviceBusMessage.Deserialize<Models.User>();

            Assert.Equal("Jane Doe", model.Name);
        }

        [Fact]
        public void ShouldConvertMessage()
        {
            var userMessage = new Models.User
            {
                Category = "UserCategory",
                CreatedAt = DateTime.Now,
                Id = "UserId1",
                Name = "Jane Doe",
                Type = "UserType"
            };

            var serviceBusMessage = new ServiceBusMessage(userMessage.ToBytes());
            var model = serviceBusMessage.Deserialize<Models.User>();

            Assert.Equal("Jane Doe", model.Name);
        }
    }
}
