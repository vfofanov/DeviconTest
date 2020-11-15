using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace DeviconTestFunctionApp
{
    public static class CopyJsonFromPublicApiFunction
    {
        [FunctionName("CopyJsonFromPublicApiFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "copy-public-api")]
            HttpRequest req, ILogger log)
        {
            log.LogInformation("Copy public API function processed a request.");

            return await PublicApiHelper.ReadData(req, async (stream, name) =>
            {
                await using (stream)
                {
                    using var reader = new StreamReader(stream);
                    return new OkObjectResult(await reader.ReadToEndAsync());
                }
            });
        }
    }
}