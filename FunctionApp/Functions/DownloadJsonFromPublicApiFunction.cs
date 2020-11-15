using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace DeviconTestFunctionApp
{
    public static class DownloadJsonFromPublicApiFunction
    {
        [FunctionName("DownloadJsonFromPublicApiFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "download-public-api")]
            HttpRequest req, ILogger log)
        {
            log.LogInformation("Download public API function processed a request.");

            return await PublicApiHelper.ReadData(req, (stream, name) =>
                Task.FromResult((IActionResult) new FileStreamResult(stream, "application/json") {FileDownloadName = $"{name}.json"}));
        }
    }
}