using Azure.Messaging.ServiceBus;
using AzureFunction.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureFunction.Funcs
{
    public class OrderProcessor
    {
        private readonly ILogger<OrderProcessor> _logger;

        public OrderProcessor(ILogger<OrderProcessor> logger)
        {
            _logger = logger;
        }

        [Function(nameof(OrderProcessor))]
        public async Task Run(
            [ServiceBusTrigger("order.created", Connection = "ServiceBusConnection", 
            IsSessionsEnabled = true,
            AutoCompleteMessages = false)]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            _logger.LogInformation("Session: {id}", message.SessionId);
            _logger.LogInformation("Message ID: {id}", message.MessageId);
            _logger.LogInformation("Message Body: {body}", message.Body);
            _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

            try
            {
                var order = message.Body.ToObjectFromJson<Order>()!;                
                _logger.LogInformation("Message model: {model}", order);

                // complete the message
                //await messageActions.CompleteMessageAsync(message);

                await messageActions.AbandonMessageAsync(message);
                //await messageActions.DeadLetterMessageAsync(message);

                _logger.LogInformation($"Completed message ID: {message.MessageId} from session: {message.SessionId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to process message ID: {message.MessageId} from session: {message.SessionId}. Abandoning.");

                // abandon the message to place it back on the queue
                await messageActions.AbandonMessageAsync(message);

                // send message to dlq
                //await messageActions.DeadLetterMessageAsync(message: message);
            }
        }

        [Function("ProcessDeadLetter")]
        public void RunDLQ([ServiceBusTrigger("order.created/$DeadLetterQueue", Connection = "ServiceBusConnection")]
            ServiceBusReceivedMessage message)
        {
            _logger.LogInformation("Session: {id}", message.SessionId);
            _logger.LogInformation("Message ID: {id}", message.MessageId);
            _logger.LogInformation("Message Body: {body}", message.Body);
            _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

            string body = message.Body.ToString();
            _logger.LogWarning($"Mensagem na Dead Letter Queue: {body}");
        }
    }
}
