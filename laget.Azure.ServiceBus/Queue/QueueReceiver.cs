using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;

namespace laget.Azure.ServiceBus.Queue
{
    public interface IQueueReceiver
    {
        void Register(Func<IQueueClient, Microsoft.Azure.ServiceBus.Message, CancellationToken, Task> callback, Func<ExceptionReceivedEventArgs, Task> exceptionHandler, MessageHandlerOptions handlerOptions = null);
    }

    public class QueueReceiver : IQueueReceiver
    {
        private readonly IQueueClient _client;

        public QueueReceiver(string connectionString, QueueOptions options)
        {
            _client = new QueueClient(connectionString, options.QueueName, options.ReceiveMode, options.RetryPolicy);
        }


        public void Register(Func<IQueueClient, Microsoft.Azure.ServiceBus.Message, CancellationToken, Task> callback, Func<ExceptionReceivedEventArgs, Task> exceptionHandler, MessageHandlerOptions handlerOptions = null)
        {
            if (handlerOptions == null)
                handlerOptions = new MessageHandlerOptions(exceptionHandler) { MaxConcurrentCalls = 10, AutoComplete = true };

            _client.RegisterMessageHandler((msg, ct) => callback(_client, msg, ct), handlerOptions);
        }
    }
}
