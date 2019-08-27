using Qube.Core;
using System;

namespace Qube.Grpc
{
    public static class OptionsBuilderExtensions
    {
        public static StreamDbContextOptionsBuilder UseGrpcStream(this StreamDbContextOptionsBuilder builder, string url)
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
                .Invoke(new object[] { options });
            return stream;
        }
    }
}