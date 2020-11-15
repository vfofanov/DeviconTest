using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace DeviconTestFunctionApp
{
    public static class PublicApiCopyJsonFunction
    {
        [FunctionName("PublicApiCopyJsonFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "public-api-copy")]
            HttpRequest req, ExecutionContext context, ILogger log)
        {
            log.LogInformation("Copy public API function processed a request.");

            var container = await PublicBlobContainer.GetAsync(context, log);
            var helper = new PublicApiHelper(container);

            return await helper.ReadData(req, async (count, data) =>
            {
                var errorsList = new List<dynamic>();

                await foreach (var pair in data)
                {
                    if (pair.IsFailed)
                    {
                        errorsList.Add(new {name = pair.Name, error = pair.Exception.Message});
                        continue;
                    }
                    var blobName = $"{pair.Name}.json";
                    var blob = container.GetBlockBlobReference(blobName);
                    blob.Properties.ContentType = "application/json";
                    var stream = pair.Stream;
                    await using (stream)
                    {
                        await blob.UploadFromStreamAsync(stream);
                    }
                }

                var sb = new StringBuilder($"{count - errorsList.Count} blobs uploaded to container '{container.Name}' successfully.");
                if (errorsList.Count != 0)
                {
                    sb.AppendLine($" {errorsList.Count} blobs failed, see details bellow.");
                    foreach (var error in errorsList)
                    {
                        sb.AppendLine($"    {error.name} : {error.error}");
                    }
                }
                return new OkObjectResult(sb.ToString());
            });
        }
    }
}