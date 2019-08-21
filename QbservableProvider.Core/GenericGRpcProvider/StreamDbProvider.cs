using System.Linq.Expressions;
using System.Reactive.Linq;

namespace QbservableProvider.Core.GenericGRpcProvider
{
    internal class StreamDbProvider : IQbservableProvider
    {
        private readonly StreamDbContextOptions _options;

        public StreamDbProvider(StreamDbContextOptions options)
        {
            _options = options;
        }

        public IQbservable<T> CreateQuery<T>(Expression expression)
        {
            return new Stream<T>(this, expression, _options);
        }
    }
}
