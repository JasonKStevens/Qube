using System;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reflection;

namespace QbservableProvider.Core
{
    public class StreamDbContextOptions
    {
        private Func<StreamDbContextOptions, Type, object> _streamFactory;

        public IHttpClientFactory HttpClientFactor { get; set; } = new DefaultHttpClientFactory();
        public string ConnectionString { get; private set; }

        internal void SetStreamFactory(Func<StreamDbContextOptions, Type, object> streamFactory)
        {
            _streamFactory = streamFactory;
        }

        internal void HttpClientFactory(IHttpClientFactory httpClientFactory)
        {
            HttpClientFactor = httpClientFactory;
        }

        internal void SetConnectionString(string url)
        {
            ConnectionString = url;
        }

        internal IQbservable<TIn> CreateStream<TIn>()
        {
            return (IQbservable<TIn>) _streamFactory(this, typeof(TIn));
        }
    }
}
