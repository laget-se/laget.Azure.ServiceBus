using Azure.Messaging.ServiceBus;

namespace laget.Azure.ServiceBus.Topic
{
    public class TopicOptions
    {
        public string TopicName { get; set; }
        public string SubscriptionName { get; set; }

        public ServiceBusClientOptions ServiceBusClientOptions { get; set; } = new ServiceBusClientOptions();
        public ServiceBusProcessorOptions ServiceBusProcessorOptions { get; set; } = new ServiceBusProcessorOptions()
        {
            AutoCompleteMessages = true
        };
    }
}
