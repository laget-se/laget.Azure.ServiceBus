using Azure.Messaging.ServiceBus;
using System.Collections.Generic;

namespace laget.Azure.ServiceBus.Extensions
{
    public static class QueueExtensions
    {
        public static void Enqueue(this Queue<ServiceBusMessage> queue, IEnumerable<IMessage> messages)
        {

            // foreach (var message in messages)
            //     queue.Enqueue(await CreateMessage(message));
        }
    }
}
