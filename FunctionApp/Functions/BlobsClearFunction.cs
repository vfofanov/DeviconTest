using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace DeviconTestFunctionApp
{
    public static class BlobsClearFunction
    {
        [FunctionName("BlobsClearFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "blobs-clear")]
            HttpRequest req, ExecutionContext context, ILogger log)
        {
            log.LogInformation("Clear blobs function processed a request.");

            var container = await PublicBlobContainer.GetAsync(context, log);
            await container.DeleteIfExistsAsync();
            return new OkResult();
        }
    }
}