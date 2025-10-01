using Azure.Messaging.ServiceBus;

namespace WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly ServiceBusProcessor _busProcessor;
        private readonly ServiceBusSessionProcessor _sessionProcessor;

        public Worker(ILogger<Worker> logger,
            IConfiguration configuration)
        {
            _logger = logger;

            var connectionString = configuration["ServiceBusConnection"];
            var queueName = configuration["ServiceBusQueueName"];

            _serviceBusClient = new ServiceBusClient(connectionString);

            // Create a processor for a session-enabled queue
            _sessionProcessor = _serviceBusClient.CreateSessionProcessor(queueName, new ServiceBusSessionProcessorOptions
            {
                MaxConcurrentSessions = 1, // Process one session at a time for strict FIFO within a session
                MaxConcurrentCallsPerSession = 1, // Process one message at a time within a session
                AutoCompleteMessages = false // Manually complete messages
            });            

            _sessionProcessor.ProcessMessageAsync += ProcessSessionMessageAsync;
            _sessionProcessor.ProcessErrorAsync += ProcessErrorAsync;

            /*
            _busProcessor = _serviceBusClient.CreateProcessor(queueName, new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = false // Manually complete messages
            });

            _busProcessor.ProcessMessageAsync += ProcessMessageAsync;
            _busProcessor.ProcessErrorAsync += ProcessErrorAsync;
            */
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    if (_logger.IsEnabled(LogLevel.Information))
            //    {
            //        _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            //    }
            //    await Task.Delay(1000, stoppingToken);
            //}

            _logger.LogInformation("Service Bus Session Worker starting.");
            await _sessionProcessor.StartProcessingAsync(stoppingToken);
            //await _busProcessor.StartProcessingAsync(stoppingToken);

            // Keep the service running until cancellation is requested
            await Task.Delay(Timeout.Infinite, stoppingToken);

            _logger.LogInformation("Service Bus Session Worker stopping.");
            await _sessionProcessor.StopProcessingAsync(stoppingToken);
            //await _busProcessor.StopProcessingAsync(stoppingToken);
        }

        private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
        {
            _logger.LogInformation($"Received message, SequenceNumber: {args.Message.SequenceNumber}");

            try
            {
                // Simulate message processing
                //await Task.Delay(TimeSpan.FromSeconds(2));

                // Process the message body
                var messageBody = args.Message.Body.ToString();
                _logger.LogInformation($"Processing message: {messageBody}");

                // Complete the message, indicating successful processing
                await args.CompleteMessageAsync(args.Message);
                _logger.LogInformation($"Completed message, SequenceNumber: {args.Message.SequenceNumber}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing message, SequenceNumber: {args.Message.SequenceNumber}");
                // Abandon the message, making it available for redelivery after a lock duration
                await args.AbandonMessageAsync(args.Message);
            }
        }

        private async Task ProcessSessionMessageAsync(ProcessSessionMessageEventArgs args)
        {
            _logger.LogInformation($"Received message from session: {args.SessionId}, SequenceNumber: {args.Message.SequenceNumber}");

            try
            {
                // Simulate message processing
                //await Task.Delay(TimeSpan.FromSeconds(2));

                // Process the message body
                var messageBody = args.Message.Body.ToString();
                _logger.LogInformation($"Processing message: {messageBody}");

                // Complete the message, indicating successful processing
                await args.CompleteMessageAsync(args.Message);
                _logger.LogInformation($"Completed message from session: {args.SessionId}, SequenceNumber: {args.Message.SequenceNumber}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing message from session: {args.SessionId}, SequenceNumber: {args.Message.SequenceNumber}");
                // Abandon the message, making it available for redelivery after a lock duration
                await args.AbandonMessageAsync(args.Message);
            }
        }

        private Task ProcessErrorAsync(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception, $"Error in Service Bus session processor: {args.FullyQualifiedNamespace}, {args.EntityPath}");
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _sessionProcessor.DisposeAsync().AsTask().Wait();
            //_busProcessor.DisposeAsync().AsTask().Wait();
            _serviceBusClient.DisposeAsync().AsTask().Wait();
            base.Dispose();
        }
    }
}
