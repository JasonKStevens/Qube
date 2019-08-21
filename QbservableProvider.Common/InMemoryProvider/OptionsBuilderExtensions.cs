using System;
using System.Reactive.Linq;

namespace QbservableProvider.Core.InMemoryProvider
{
    public static class OptionsBuilderExtensions
    {
        public static StreamDbContextOptionsBuilder UseInMemoryStream<T>(this StreamDbContextOptionsBuilder builder, IObservable<T> observable)
        {
            builder.Options.SetStreamFactory((options, type) =>
            {
                return new Stream<T>(observable, options);
            });
            return builder;
        }
    }
}
