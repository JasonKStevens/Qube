using System;

namespace QbservableProvider.Core.GenericGRpcProvider
{
    public static class OptionsBuilderExtensions
    {
        public static StreamDbContextOptionsBuilder UseGenericGRpcStreams(this StreamDbContextOptionsBuilder builder, string url)
        {
            builder.Options.SetConnectionString(url);
            builder.Options.SetStreamFactory(CreateStream);
            return builder;
        }

        private static object CreateStream(StreamDbContextOptions options, Type genericParam)
        {
            var stream = typeof(Stream<>)
                .MakeGenericType(new[] { genericParam })
                .GetConstructor(new[] { typeof(StreamDbContextOptions) })
                .Invoke(new[] { options });
            return stream;
        }
    }
}