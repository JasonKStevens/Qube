using System;
using Qube.Core;
using Qube.Grpc;

namespace Qube.EventStore
{
    public static class OptionsBuilderExtensions
    {
        public static StreamDbContextOptionsBuilder UseEventStore(this StreamDbContextOptionsBuilder builder, string url)
        {
            builder.Options.SetConnectionString(url);
            builder.Options.SetStreamFactory(CreateStream);
            return builder;
        }

        private static object CreateStream(StreamDbContextOptions options, Type genericParam, string[] streamPatterns)
        {
            var stream = typeof(Stream<>)
                .MakeGenericType(new[] { genericParam })
                .GetConstructor(new[] { typeof(StreamDbContextOptions), typeof(string[]) })
                .Invoke(new object[] { options, streamPatterns });
            return stream;
        }
    }
}