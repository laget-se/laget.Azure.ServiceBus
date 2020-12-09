using Microsoft.Azure.ServiceBus;

namespace laget.Azure.Queue
{
    public class QueueOptions
    {
        public string QueueName { get; set; }
        public ReceiveMode ReceiveMode { get; set; } = ReceiveMode.PeekLock;
        public RetryPolicy RetryPolicy { get; set; } = RetryPolicy.Default;
    }
}
