using System.Linq.Expressions;
using System.Net.Http;
using System.Reactive.Linq;

namespace QbservableProvider.Core.GRpcProvider
{
    internal class StreamDbProvider<TIn> : IQbservableProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly StreamDbContextOptions _options;

        public StreamDbProvider(IHttpClientFactory httpClientFactory, StreamDbContextOptions options)
        {
            _httpClientFactory = httpClientFactory;
            _options = options;
        }

        public IQbservable<TOut> CreateQuery<TOut>(Expression expression)
        {
            return new QueryStream<TIn, TOut>(this, expression, _httpClientFactory, _options);
        }
    }
}
