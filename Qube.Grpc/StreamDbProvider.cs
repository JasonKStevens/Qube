using Qube.Core;
using System.Linq.Expressions;
using System.Reactive.Linq;

namespace Qube.Grpc
{
    internal class StreamDbProvider<TIn> : IQbservableProvider
    {
        private readonly StreamDbContextOptions _options;
        private readonly string[] _streamPatterns;

        public StreamDbProvider(StreamDbContextOptions options, string[] streamPatterns)
        {
            _options = options;
            _streamPatterns = streamPatterns;
        }

        public IQbservable<TOut> CreateQuery<TOut>(Expression expression)
        {
            return new QueryStream<TIn, TOut>(this, expression, _options, _streamPatterns);
        }
    }
}
