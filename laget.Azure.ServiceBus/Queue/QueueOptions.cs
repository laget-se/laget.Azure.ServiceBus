
using Azure.Messaging.ServiceBus;

namespace laget.Azure.ServiceBus.Queue
{
    public class QueueOptions
    {
        public string QueueName { get; set; }

        public ServiceBusClientOptions ServiceBusClientOptions { get; set; } = new ServiceBusClientOptions();
    }
}
