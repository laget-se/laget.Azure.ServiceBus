using Microsoft.Azure.ServiceBus;

namespace laget.Azure.ServiceBus.Topic
{
    public class TopicOptions
    {
        public string TopicName { get; set; }
        public RetryPolicy RetryPolicy { get; set; } = RetryPolicy.Default;

        public string SubscriptionName { get; set; }
        public ReceiveMode ReceiveMode { get; set; } = ReceiveMode.PeekLock;
    }
}
