using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
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
            HttpRequest req, ExecutionContext context, ILogger log)
        {
            log.LogInformation("Read public API function processed a request.");

            var container = await PublicBlobContainer.GetAsync(context, log);
            var helper = new PublicApiHelper(container);

            return await helper.ReadData(req, async (count, data) =>
            {
                var sb = new StringBuilder();
                sb.AppendLine("[");
                var delimiter = " ";
                await foreach (var body in GetBodiesAsync(data))
                {
                    sb.Append(delimiter);
                    delimiter = ",";
                    sb.AppendLine(body);
                }
                sb.Remove(sb.Length - 1, 1);
                sb.Append("]");
                return new OkObjectResult(sb.ToString());
            });
        }

        private static async IAsyncEnumerable<string> GetBodiesAsync(IAsyncEnumerable<ApiEndpointResult> data)
        {
            await foreach (var pair in data)
            {
                var name = pair.Name;
                string body;
                if (pair.IsFailed)
                {
                    body = JsonSerializer.Serialize($"ERROR: {pair.Exception.Message}");
                }
                else
                {
                    await using var stream = pair.Stream;
                    using var reader = new StreamReader(stream);
                    body = await reader.ReadToEndAsync();    
                }
                yield return $@"{{""name"":""{name}"",""body"":{body}}}";
            }
        }
    }
}