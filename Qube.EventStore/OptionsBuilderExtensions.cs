using Qube.Core;
using System;

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