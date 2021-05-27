using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace laget.Azure.ServiceBus.Topic
{
    public interface ITopicReceiver
    {
        void Register(Func<Microsoft.Azure.ServiceBus.Message, CancellationToken, Task> callback, Func<ExceptionReceivedEventArgs, Task> exceptionHandler, MessageHandlerOptions handlerOptions = null);
    }

    public class TopicReceiver : ITopicReceiver
    {
        private readonly IMessageReceiver _client;

        public TopicReceiver(string connectionString, TopicOptions options)
        {
            _client = new MessageReceiver(connectionString, EntityNameHelper.FormatSubscriptionPath(options.TopicName, options.SubscriptionName), options.ReceiveMode, options.RetryPolicy);
        }


        public void Register(Func<Microsoft.Azure.ServiceBus.Message, CancellationToken, Task> callback, Func<ExceptionReceivedEventArgs, Task> exceptionHandler, MessageHandlerOptions handlerOptions = null)
        {
            if (handlerOptions == null)
                handlerOptions = new MessageHandlerOptions(exceptionHandler) { MaxConcurrentCalls = 10, AutoComplete = true };

            _client.RegisterMessageHandler(callback, handlerOptions);
        }
    }
}
