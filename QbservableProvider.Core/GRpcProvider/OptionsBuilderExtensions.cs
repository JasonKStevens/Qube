using System;
using System.Net.Http;

namespace QbservableProvider.Core.GRpcProvider
{
    public static class OptionsBuilderExtensions
    {
        public static StreamDbContextOptionsBuilder UseGRpcStream(this StreamDbContextOptionsBuilder builder, string url)
        {
            builder.Options.SetConnectionString(url);
            builder.Options.SetStreamFactory(CreateStream);
            return builder;
        }

        public static StreamDbContextOptionsBuilder UseHttpClientFactory(this StreamDbContextOptionsBuilder builder, IHttpClientFactory httpClientFactory)
        {
            builder.Options.HttpClientFactory(httpClientFactory);
            return builder;
        }

        private static object CreateStream(StreamDbContextOptions options, Type genericParam)
        {
            // TEMP: Sort this out
            var httpClientFactory = new DefaultHttpClientFactory();

            var stream = typeof(Stream<>)
                .MakeGenericType(new[] { genericParam })
                .GetConstructor(new[] { typeof(IHttpClientFactory), typeof(StreamDbContextOptions) })
                .Invoke(new object[] { httpClientFactory, options });
            return stream;
        }
    }
}