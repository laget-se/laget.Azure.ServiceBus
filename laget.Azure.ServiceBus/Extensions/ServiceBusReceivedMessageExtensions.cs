using Azure.Messaging.ServiceBus;
using System;

namespace laget.Azure.ServiceBus.Extensions
{
    public static class ServiceBusReceivedMessageExtensions
    {
        public static DateTime ExpiresAt(this ServiceBusReceivedMessage message)
        {
            return message.ExpiresAt.LocalDateTime;
        }

        public static DateTime ExpiresAtUtc(this ServiceBusReceivedMessage message)
        {
            return message.ExpiresAt.UtcDateTime;
        }

        public static DateTime ScheduledAt(this ServiceBusReceivedMessage message)
        {
            if (message.ScheduledEnqueueTime == DateTimeOffset.MinValue ||
                message.ScheduledEnqueueTime == DateTimeOffset.MaxValue ||
                message.ScheduledEnqueueTime == new DateTimeOffset(1970, 1, 1, 0, 0, 0, new TimeSpan(0, 0, 0, 0)))
            {
                return message.EnqueuedTime.LocalDateTime;
            }

            return message.ScheduledEnqueueTime.LocalDateTime;
        }

        public static DateTime ScheduledAtUtc(this ServiceBusReceivedMessage message)
        {
            if (message.ScheduledEnqueueTime == DateTimeOffset.MinValue ||
                message.ScheduledEnqueueTime == DateTimeOffset.MaxValue ||
                message.ScheduledEnqueueTime == new DateTimeOffset(1970, 1, 1, 0, 0, 0, new TimeSpan(0, 0, 0, 0)))
            {
                return message.EnqueuedTime.DateTime;
            }

            return message.ScheduledEnqueueTime.DateTime;
        }

        internal static ServiceBusMessage ToServiceBudMessage(this ServiceBusReceivedMessage message)
        {
            return new ServiceBusMessage(message.GetRawAmqpMessage());
        }
    }
}
