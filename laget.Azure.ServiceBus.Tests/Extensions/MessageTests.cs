using System.Text;
using laget.Azure.ServiceBus.Extensions;
using Xunit;

namespace laget.Azure.ServiceBus.Tests.Extensions
{
    public class MessageTests
    {
        [Fact]
        public void ShouldDeserializeMessage()
        {
            var body = Encoding.UTF8.GetBytes("{ \"name\": \"Jane Doe\" }");
            var message = new Microsoft.Azure.ServiceBus.Message { Body = body };

            var model = message.Deserialize<Models.User>();

            Assert.Equal("Jane Doe", model.Name);
        }
    }
}
