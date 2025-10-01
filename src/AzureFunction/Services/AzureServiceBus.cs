using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using AzureFunction.Funcs;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace AzureFunction.Services
{
    public class AzureServiceBus
    {
        private readonly ILogger _logger;
        private readonly ServiceBusSender _sender;
        private readonly string _connectionString;
        private readonly string _queueName;

        public AzureServiceBus(string connectionString, string queueName)
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });
            _logger = loggerFactory.CreateLogger(nameof(AzureServiceBus));

            var retryOptions = new ServiceBusRetryOptions
            {
                MaxRetries = 5, // Set the maximum number of retries
                Mode = ServiceBusRetryMode.Exponential, // Use exponencial backoff for retries
                MaxDelay = TimeSpan.FromSeconds(10), // Maximum delay between retries
                Delay = TimeSpan.FromSeconds(10) // Initial delay between retries
            };
            var clientOptions = new ServiceBusClientOptions
            {
                TransportType = ServiceBusTransportType.AmqpTcp,
                RetryOptions = retryOptions,
            };            

            var client = new ServiceBusClient(connectionString, clientOptions);
            _sender = client.CreateSender(queueName);

            _connectionString = connectionString;
            _queueName = queueName;
        }

        private async Task CreateQueueAsync()
        {
            var administrationClient = new ServiceBusAdministrationClient(_connectionString);
            if (!await administrationClient.QueueExistsAsync(_queueName))
            {
                var options = new CreateQueueOptions(_queueName)
                {
                    RequiresSession = true,
                    MaxDeliveryCount = 3,
                };
                await administrationClient.CreateQueueAsync(options);
            }
        }

        public async Task SendSessionMessagesAsync(string sessionId, params string[] messages)
        {
            var messagesBatch = await _sender.CreateMessageBatchAsync();

            foreach (var message in messages)
            {
                try
                {
                    var data = Encoding.UTF8.GetBytes(message);
                    //var data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(messageText));
                    var serviceBusMessage = new ServiceBusMessage(data)
                    {
                        SessionId = sessionId // Assign the same SessionId for FIFO within this group
                    };

                    messagesBatch.TryAddMessage(serviceBusMessage);
                    _logger.LogInformation($"Sent message: {message} with SessionId: {sessionId}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error sending message: {ex.Message}");
                }
            }

            await _sender.SendMessagesAsync(messagesBatch);

            await _sender.CloseAsync();
        }        

        public async Task SendSessionMessagesAsync(string sessionId, object data)
        {
            try
            {
                await CreateQueueAsync();

                var @byte = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data));
                var message = new ServiceBusMessage(@byte)
                {
                    SessionId = sessionId // Assign the same SessionId for FIFO within this group
                };
                await _sender.SendMessageAsync(message);

                _logger.LogInformation($"Sent message: {Convert.ToString(data)} with SessionId: {sessionId}");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Error sending message: {ex.Message}");
                throw;
            }
            finally
            {
                await _sender.CloseAsync();
            }
        }
    }
}
