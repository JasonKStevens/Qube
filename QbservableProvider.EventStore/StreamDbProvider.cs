using System.Linq.Expressions;
using System.Net.Http;
using System.Reactive.Linq;
using QbservableProvider.Core;

namespace QbservableProvider.EventStore
{
    internal class StreamDbProvider<TIn> : IQbservableProvider
    {
        private readonly StreamDbContextOptions _options;

        public StreamDbProvider(StreamDbContextOptions options)
        {
            _options = options;
        }

        public IQbservable<TOut> CreateQuery<TOut>(Expression expression)
        {
            return new QueryStream<TIn, TOut>(this, expression, _options);
        }
    }
}
