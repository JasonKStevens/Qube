using LiteGuard;
using System;
using System.Linq.Expressions;
using System.Reactive.Linq;

namespace QbservableProvider.Core.GenericGRpcProvider
{
    internal class Stream<TOut> : IQbservable<TOut>
    {
        private readonly StreamDbContextOptions _options;

        public Type ElementType { get; private set; }
        public Expression Expression { get; private set; }
        public IQbservableProvider Provider { get; private set; }

        // Called by stream factory given to context options
        public Stream(StreamDbContextOptions options)
        {
            _options = options;

            ElementType = typeof(TOut);
            Provider = new StreamDbProvider(options);  // Provider will create stream in constructor below if there's a linq query involved
            Expression = SerializationHelper.NewObserverParameter<TOut>();
        }

        // Stream is created a second time by qbservable provider when linq is used.
        public Stream(IQbservableProvider provider, Expression expression, StreamDbContextOptions options)
        {
            ElementType = typeof(TOut);
            Provider = provider;
            Expression = expression;
            _options = options;
        }

        public IDisposable Subscribe(IObserver<TOut> observer)
        {
            var sub = new Subscription(Expression, _options);
            // TODO: Make event genric - will need to get it from Expression
            sub.Attach<Event, TOut>(observer);
            return sub;
        }
    }
}
