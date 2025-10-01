using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AzureFunction.Middlewares
{
    public class CustomMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly ILogger<CustomMiddleware> _logger;

        public CustomMiddleware(ILogger<CustomMiddleware> logger)
        {
            _logger = logger;
        }

        public async  Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            _logger.LogInformation("middleware");

            try
            {
                await next(context);
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "middleware exception");

                var request = await context.GetHttpRequestDataAsync();
                var response = request!.CreateResponse(HttpStatusCode.InternalServerError);

                var errorMessage = new { status = "fault", message = "An unhandled exception occured.", exception = ex.Message, ex.StackTrace };

                await response.WriteAsJsonAsync(errorMessage);

                context.GetInvocationResult().Value = response;
            } 
        }
    }
}
