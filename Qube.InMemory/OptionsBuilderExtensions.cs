using Qube.Core;
using System;

namespace Qube.InMemory
{
    public static class OptionsBuilderExtensions
    {
        public static StreamDbContextOptionsBuilder UseInMemoryStream<T>(this StreamDbContextOptionsBuilder builder, IObservable<T> observable)
        {
            builder.Options.SetStreamFactory((options, type, streamPatterns) =>
            {
                return new Stream<T>(observable, options, streamPatterns);
            });
            return builder;
        }
    }
}
