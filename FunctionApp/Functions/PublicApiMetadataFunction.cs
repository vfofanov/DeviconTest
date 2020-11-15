using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace DeviconTestFunctionApp
{
    public static class PublicApiMetadataFunction
    {
        [FunctionName("PublicApiMetadataFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "public-api-metadata")]
            HttpRequest req, ExecutionContext context, ILogger log)
        {
            log.LogInformation("Clear blobs function processed a request.");

            var container = await PublicBlobContainer.GetAsync(context, log);
            var helper = new PublicApiHelper(container);

            return new OkObjectResult(await helper.GetMetadataAsync());
        }
    }
}