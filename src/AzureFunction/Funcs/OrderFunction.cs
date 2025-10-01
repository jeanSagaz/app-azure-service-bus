using AzureFunction.Model;
using AzureFunction.Services;
using AzureFunction.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureFunction.Funcs
{
    public class OrderFunction
    {
        private readonly ILogger<OrderFunction> _logger;

        public OrderFunction(ILogger<OrderFunction> logger)
        {
            _logger = logger;
        }

        [Function(nameof(OrderFunction))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
        {
            var order = await req.ReadFromJsonAsync<Order>();

            //throw new Exception("it went wrong");

            var connectionString = ConfigurationUtil.GetConfiguration("ServiceBusConnection");
            var queueName = "order.created";
            var azureServiceBus = new AzureServiceBus(connectionString: connectionString, queueName: queueName);
            await azureServiceBus.SendSessionMessagesAsync(sessionId: queueName, data: order!);

            _logger.LogInformation("C# HTTP trigger function processed a request.");
            return new OkObjectResult(order);
        }
    }
}
