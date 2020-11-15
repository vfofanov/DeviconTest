using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage.Blob;

namespace DeviconTestFunctionApp
{
    public class PublicApiHelper
    {
        private static readonly Uri PublicApisBaseUri = new Uri("https://sampleapis.com");
        private readonly ApiWorker _apiWorker;
        private readonly CloudBlobContainer _metadataContainer;
        private const string MetadataBlobName = "_metadata.json";

        private static readonly Regex MetadataRegex = new Regex(
            @"const\s+apiList\s*=\s*(.+?)\s*;",
            RegexOptions.IgnoreCase
            | RegexOptions.Singleline
            | RegexOptions.CultureInvariant
            | RegexOptions.Compiled
        );


        public PublicApiHelper(CloudBlobContainer metadataContainer)
        {
            _metadataContainer = metadataContainer;
            _apiWorker = new ApiWorker(PublicApisBaseUri);
        }

        public async Task<IActionResult> ReadData(HttpRequest req, Func<int, IAsyncEnumerable<ApiEndpointResult>, Task<IActionResult>> func)
        {
            var api = req.Query["api"].ToString()?.ToLowerInvariant();

            var endpoints = req.Query["endpoint"].ToString()?.ToLowerInvariant().Split(',') ?? new string[0];

            try
            {
                List<Tuple<string, string>> apiEndpointPairs;

                if (!string.IsNullOrEmpty(api) && endpoints.Length != 0)
                {
                    apiEndpointPairs = endpoints.Where(e => !string.IsNullOrWhiteSpace(e)).Select(e => new Tuple<string, string>(api, e)).ToList();
                }
                else
                {
                    var metadata = await GetMetadataAsync();
                    var apis = string.IsNullOrEmpty(api) ? metadata : metadata.Where(m => m.api == api);
                    apiEndpointPairs = apis.SelectMany(a => a.endpoints, (m, ep) => new Tuple<string, string>(m.api, ep)).ToList();
                }

                return await func(apiEndpointPairs.Count, ReadApiEndpoints(apiEndpointPairs));
            }
            catch (HttpRequestException e)
            {
                return new BadRequestErrorMessageResult(e.Message);
            }
        }

        private async IAsyncEnumerable<ApiEndpointResult> ReadApiEndpoints(IEnumerable<Tuple<string, string>> apiEndpointPairs)
        {
            foreach (var pair in apiEndpointPairs)
            {
                var api = pair.Item1;
                var endpoint = pair.Item2;

                Exception exc = null;
                Stream stream=null;

                try
                {
                    stream = await _apiWorker.ReadBodyAsync($"{api}/api/{endpoint}");
                }
                catch (HttpRequestException e)
                {
                    exc = e;
                }


                yield return new ApiEndpointResult($"{api}-{endpoint}", stream, exc);
            }
        }


        /// <summary>
        ///     Get apis metadata.
        /// </summary>
        /// <returns></returns>
        public async Task<PublicApiMetadata[]> GetMetadataAsync(bool reload = false) {
            var metadataBlob = _metadataContainer?.GetBlockBlobReference(MetadataBlobName);

            if (metadataBlob != null && !reload)
            {
                if (await metadataBlob.ExistsAsync())
                {
                    var json = await metadataBlob.DownloadTextAsync();
                    try
                    {
                        return JsonSerializer.Deserialize<PublicApiMetadata[]>(json);
                    }
                    catch (JsonException)
                    {
                        //skip and reread metadata
                    }
                }
            }

            var result = (await ReadMetadataAsync()).ToArray();

            if (metadataBlob != null)
            {
                metadataBlob.Properties.ContentType = "application/json";
                await metadataBlob.UploadTextAsync(JsonSerializer.Serialize(result));
            }
            return result;
        }

        private async Task<List<PublicApiMetadata>> ReadMetadataAsync()
        {
            var apiPage = await _apiWorker.ReadBodyTextAsync();
            var match = MetadataRegex.Match(apiPage);
            if (!match.Success)
            {
                return new List<PublicApiMetadata>();
            }
            var apisJson = match.Groups[1].Value;
            var apiArray = JsonSerializer.Deserialize<JsonElement>(apisJson);
            var result = new List<PublicApiMetadata>();
            foreach (var item in apiArray.EnumerateArray())
            {
                var metadata = new PublicApiMetadata();
                foreach (var property in item.EnumerateObject())
                {
                    if (property.NameEquals("link"))
                    {
                        metadata.api = property.Value.GetString();
                    }
                    if (property.NameEquals("endPoints"))
                    {
                        metadata.endpoints = property.Value.EnumerateArray().Select(endPoint => endPoint.GetString()).ToArray();
                    }
                }
                result.Add(metadata);
            }

            return result;
        }
    }
}