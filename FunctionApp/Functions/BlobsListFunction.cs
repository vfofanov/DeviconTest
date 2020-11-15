using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;

namespace DeviconTestFunctionApp
{
    public static class BlobsListFunction
    {
        [FunctionName("BlobsListFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "blobs-list")]
            HttpRequest req, ExecutionContext context, ILogger log)
        {
            log.LogInformation("List blobs function processed a request.");

            var container = await PublicBlobContainer.GetAsync(context, log);

            var list = new List<string>();
            await foreach (var blobItem in container.GetAllBlobs())
            {
                list.Add(container.Uri.MakeRelativeUri(blobItem.Uri).ToString());
            }
            return new OkObjectResult(list);
        }

        private static async IAsyncEnumerable<IListBlobItem> GetAllBlobs(this CloudBlobContainer container)
        {
            BlobContinuationToken blobContinuationToken = null;
            do
            {
                var resultSegment = await container.ListBlobsSegmentedAsync(
                    currentToken: blobContinuationToken,
                    useFlatBlobListing: true,
                    blobListingDetails: BlobListingDetails.Metadata,
                    prefix: null,
                    maxResults: null,
                    options: null,
                    operationContext: null
                );
                blobContinuationToken = resultSegment.ContinuationToken;
                foreach (var item in resultSegment.Results)
                {
                    yield return item;
                }
            } while (blobContinuationToken != null);
        }
    }
}