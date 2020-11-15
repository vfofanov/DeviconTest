using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace DeviconTestFunctionApp
{
    internal class ApiWorker : IApiWorker
    {
        private readonly HttpClient _httpClient;

        public ApiWorker(Uri baseAddress)
        {
            _httpClient = new HttpClient {BaseAddress = baseAddress};
        }

        public async Task<Stream> ReadBodyAsync(string requestUri)
        {
            try
            {
                return await _httpClient.GetStreamAsync(requestUri);
            }
            catch (HttpRequestException e)
            {
                var uri = new Uri(_httpClient.BaseAddress, requestUri);
                throw new HttpRequestException($"Error during reading data from '{uri}'. Error: {e.Message}", e) {HResult = e.HResult};
            }
            
        }
    }


    public interface IApiWorker
    {
        Task<Stream> ReadBodyAsync(string requestUri);
    }
}