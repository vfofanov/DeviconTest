using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace DeviconTestFunctionApp
{
    public static class CopyJsonFromPublicApiFunction
    {
        [FunctionName("CopyJsonFromPublicApiFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "copy-public-api")]
            HttpRequest req
            , ExecutionContext executionContext, ILogger log)
        {
            log.LogInformation("Copy public API function processed a request.");

            const string blobContainerName = "public-api-files";
            var storageAccount = GetCloudStorageAccount(executionContext);
            var blobClient = storageAccount.CreateCloudBlobClient();

            if (await CreateContainerIfNotExistsTaskAsync(blobClient, blobContainerName))
            {
                log.LogInformation($"Blob container '{blobContainerName}' created");
            }
            var container = blobClient.GetContainerReference(blobContainerName);


            return await PublicApiHelper.ReadData(req, async (stream, name) =>
            {
                await using (stream)
                {
                    var blobName = $"{name}.json";
                    var blob = container.GetBlockBlobReference(blobName);
                    blob.Properties.ContentType = "application/json";
                    await blob.UploadFromStreamAsync(stream);

                    return new OkObjectResult($"Blob '{blobName}' uploaded to container '{blobContainerName}' successfully");
                }
            });
        }

        private static CloudStorageAccount GetCloudStorageAccount(ExecutionContext executionContext)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(executionContext.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", true, true)
                .AddEnvironmentVariables().Build();

            return CloudStorageAccount.Parse(config["AzureWebJobsStorage"]);
        }

        private static Task<bool> CreateContainerIfNotExistsTaskAsync(CloudBlobClient blobClient, string blobContainerName)
        {
            var blobContainer = blobClient.GetContainerReference(blobContainerName);
            return blobContainer.CreateIfNotExistsAsync();
        }
    }
}