
using Azure.Messaging.ServiceBus;

namespace laget.Azure.ServiceBus.Queue
{
    public class QueueOptions
    {
        public bool AutoCompleteMessages { get; set; } = false;
        public string QueueName { get; set; }

        public ServiceBusClientOptions ServiceBusClientOptions { get; set; } = new ServiceBusClientOptions();
        public ServiceBusProcessorOptions ServiceBusProcessorOptions { get; set; } = new ServiceBusProcessorOptions()
        {
            AutoCompleteMessages = true
        };
    }
}
