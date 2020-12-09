using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;

namespace laget.Azure.ServiceBus.Topic
{
    public interface ITopicSender
    {
        Task SendAsync(IMessage message);
    }

    public class TopicSender : ITopicSender
    {
        readonly ITopicClient _client;

        public TopicSender(string connectionString, TopicOptions options)
        {
            _client = new TopicClient(connectionString, options.TopicName, options.RetryPolicy);
        }


        public async Task SendAsync(IMessage message)
        {
            var json = message.Serialize();
            var bytes = Encoding.UTF8.GetBytes(json);

            await _client.SendAsync(new Microsoft.Azure.ServiceBus.Message(bytes));
        }
    }
}
