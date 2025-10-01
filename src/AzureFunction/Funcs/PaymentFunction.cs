using AzureFunction.Model;
using AzureFunction.Services;
using AzureFunction.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureFunction.Funcs
{
    public class PaymentFunction
    {
        private readonly ILogger<PaymentFunction> _logger;

        public PaymentFunction(ILogger<PaymentFunction> logger)
        {
            _logger = logger;
        }

        [Function(nameof(PaymentFunction))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
        {
            var payment = await req.ReadFromJsonAsync<Payment>();

            //throw new Exception("it went wrong");

            var connectionString = ConfigurationUtil.GetConfiguration("ServiceBusConnection");
            var queueName = "payment.created";
            var azureServiceBus = new AzureServiceBus(connectionString: connectionString, queueName: queueName);
            await azureServiceBus.SendSessionMessagesAsync(sessionId: queueName, data: payment!);

            _logger.LogInformation("C# HTTP trigger function processed a request.");
            return new OkObjectResult(payment);
        }
    }
}
