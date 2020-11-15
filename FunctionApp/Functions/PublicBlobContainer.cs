using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;

namespace DeviconTestFunctionApp
{
    public static class PublicBlobContainer
    {
        public const string BlobContainerName = "public-api-files";

        public static async Task<CloudBlobContainer> GetAsync(ExecutionContext context, ILogger log)
        {
            var blobHelper = new BlobContainerHelper(context, log);
            return await blobHelper.GetContainerAsync(BlobContainerName);
        }
    }
}