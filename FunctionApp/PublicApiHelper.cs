using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeviconTestFunctionApp
{
    internal static class PublicApiHelper
    {
        private static readonly Uri PublicApisBaseUri = new Uri("https://sampleapis.com");
        public static async Task<IActionResult> ReadData(HttpRequest req, Func<Stream, string, Task<IActionResult>> func)
        {
            string api = req.Query["api"];
            string endpoint = req.Query["endpoint"];

            if (string.IsNullOrEmpty(api))
            {
                return new BadRequestErrorMessageResult("Missing 'api' parameter");
            }
            if (string.IsNullOrEmpty(endpoint))
            {
                return new BadRequestErrorMessageResult("Missing 'endpoint' parameter");
            }
            var apiWorker = new ApiWorker(PublicApisBaseUri);
            try
            {
                var stream = await apiWorker.ReadBodyAsync($"{api}/api/{endpoint}");
                return await func(stream, $"{api}-{endpoint}");
            }
            catch (HttpRequestException e)
            {
                return new BadRequestErrorMessageResult(e.Message);
            }
        }
    }
}