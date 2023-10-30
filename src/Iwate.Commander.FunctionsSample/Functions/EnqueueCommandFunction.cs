using System.Net;
using Iwate.Commander.FunctionsSample.Commands;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Iwate.Commander.FunctionsSample.Functions
{
    public class EnqueueCommandFunction
    {
        private readonly Commander _commander;
        private readonly ILogger _logger;

        public EnqueueCommandFunction(Commander commander, ILoggerFactory loggerFactory)
        {
            _commander = commander;
            _logger = loggerFactory.CreateLogger<EnqueueCommandFunction>();
        }

        [Function("EnqueueCommandFunction")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            await _commander.EnqueueAsync("partition", "apikey", nameof(EmptyCommand), new MemoryStream(new byte[0]), CancellationToken.None);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString("Welcome to Azure Functions!");

            return response;
        }
    }
}
