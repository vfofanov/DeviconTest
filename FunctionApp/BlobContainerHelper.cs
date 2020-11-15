using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace DeviconTestFunctionApp
{
    public class BlobContainerHelper
    {
        private readonly ILogger _log;
        private readonly CloudStorageAccount _storageAccount;
        private readonly CloudBlobClient _blobClient;

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="log"></param>
        /// <param name="storageAccountSettingsName">
        ///     Name of setting in settings.json with connection string to storage account.
        ///     By default use the same account as functions
        /// </param>
        public BlobContainerHelper(ExecutionContext context, ILogger log, string storageAccountSettingsName = "AzureWebJobsStorage")
        {
            _log = log;
            _storageAccount = GetCloudStorageAccount(context, storageAccountSettingsName);
            _blobClient = _storageAccount.CreateCloudBlobClient();
        }

        public async Task<CloudBlobContainer> GetContainerAsync(string blobContainerName)
        {
            if (await CreateContainerIfNotExistsTaskAsync(_blobClient, blobContainerName))
            {
                _log.LogInformation($"Blob container '{blobContainerName}' created");
            }
            return _blobClient.GetContainerReference(blobContainerName);
        }

        private static CloudStorageAccount GetCloudStorageAccount(ExecutionContext executionContext, string storageAccountSettingsName)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(executionContext.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", true, true)
                .AddEnvironmentVariables().Build();

            return CloudStorageAccount.Parse(config[storageAccountSettingsName]);
        }

        private static Task<bool> CreateContainerIfNotExistsTaskAsync(CloudBlobClient blobClient, string blobContainerName)
        {
            var blobContainer = blobClient.GetContainerReference(blobContainerName);
            return blobContainer.CreateIfNotExistsAsync();
        }
    }
}