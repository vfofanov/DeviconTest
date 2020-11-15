using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace DeviconTestFunctionApp
{
    public static class ReadJsonFromPublicApiFunction
    {
        [FunctionName("ReadJsonFromPublicApiFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "read-public-api")]
            HttpRequest req, ILogger log)
        {
            log.LogInformation("Read public API function processed a request.");

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