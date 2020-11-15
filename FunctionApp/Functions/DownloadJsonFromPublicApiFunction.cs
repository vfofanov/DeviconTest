using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
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
            HttpRequest req, ExecutionContext context, ILogger log)
        {
            log.LogInformation("Download public API function processed a request.");

            var container = await PublicBlobContainer.GetAsync(context, log);
            var helper = new PublicApiHelper(container);

            return await helper.ReadData(req, async (count, data) =>
            {
                if (count == 1)
                {
                    var enumerator = data.GetAsyncEnumerator();
                    await enumerator.MoveNextAsync();
                    var name = enumerator.Current.Name;
                    var stream = enumerator.Current.Stream;
                    return new FileStreamResult(stream, "application/json") {FileDownloadName = $"{name}.json"};
                }
                else
                {
                    var stream = await GetZipAsync(data);
                    return new FileStreamResult(stream, "application/zip") {FileDownloadName = "public_apis.zip"};
                }
            });
        }

        private static async Task<Stream> GetZipAsync(IAsyncEnumerable<ApiEndpointResult> data)
        {
            var zipStream = new MemoryStream();

            using (var zip = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                var errorsList = new List<dynamic>();

                await foreach (var pair in data)
                {
                    if (pair.IsFailed)
                    {
                        errorsList.Add(new {name = pair.Name, error = pair.Exception.Message});
                        continue;
                    }
                    var name = pair.Name;
                    await using var stream = pair.Stream;
                    var entry = zip.CreateEntry($"{name}.json");
                    await using var entryStream = entry.Open();
                    await stream.CopyToAsync(entryStream);
                }

                if (errorsList.Count != 0)
                {
                    var entry = zip.CreateEntry($"_errors.json");
                    await using var entryStream = entry.Open();
                    await JsonSerializer.SerializeAsync(entryStream, errorsList);
                }
            }
            zipStream.Position = 0;
            return zipStream;
        }
    }
}