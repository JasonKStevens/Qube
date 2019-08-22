using System.Net.Http;

namespace QbservableProvider.Core
{
    internal class DefaultHttpClientFactory : IHttpClientFactory
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        // TODO: Support named clients
        public HttpClient CreateClient(string name)
        {
            return _httpClient;
        }
    }
}
