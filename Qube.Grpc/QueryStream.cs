using Qube.Core;
using System;
using System.Linq.Expressions;
using System.Reactive.Linq;

namespace Qube.Grpc
{
    internal class QueryStream<TIn, TOut> : IQbservable<TOut>
    {
        private readonly StreamDbContextOptions _options;
        private readonly string[] _streamPatterns;

        public Type ElementType { get; private set; }
        public Expression Expression { get; private set; }
        public IQbservableProvider Provider { get; private set; }

        public QueryStream(
            IQbservableProvider provider,
            Expression expression,
            StreamDbContextOptions options,
            string[] streamPatterns)
        {
            ElementType = typeof(TIn);
            Provider = provider;
            Expression = expression;
            _options = options;
            _streamPatterns = streamPatterns;
        }

        public IDisposable Subscribe(IObserver<TOut> observer)
        {
            var sub = new Subscription(_options, _streamPatterns);
            sub.Connect<TIn, TOut>(Expression, observer);
            return sub;
        }
    }
}
