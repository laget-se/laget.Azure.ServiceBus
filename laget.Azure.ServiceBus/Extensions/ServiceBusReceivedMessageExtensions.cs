using Azure.Messaging.ServiceBus;
using System;

namespace laget.Azure.ServiceBus.Extensions
{
    public static class ServiceBusReceivedMessageExtensions
    {
        public static DateTime ExpiresAt(this ServiceBusReceivedMessage message)
        {
            return (DateTime.UtcNow + message.TimeToLive);
        }

        public static DateTime ExpiresAtUtc(this ServiceBusReceivedMessage message)
        {
            return (DateTime.UtcNow + message.TimeToLive).ToLocalTime();
        }

        public static DateTime ScheduledAt(this ServiceBusReceivedMessage message)
        {
            return message.ScheduledEnqueueTime.LocalDateTime;
        }

        public static DateTime ScheduledAtUtc(this ServiceBusReceivedMessage message)
        {
            return message.ScheduledEnqueueTime.DateTime;
        }

        public static ServiceBusMessage ToServiceBudMessage(this ServiceBusReceivedMessage message)
        {
            return new ServiceBusMessage(message.GetRawAmqpMessage());
        }
    }
}
