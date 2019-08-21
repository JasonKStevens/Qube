using LiteGuard;
using System;
using System.Linq.Expressions;
using System.Reactive.Linq;

namespace QbservableProvider.Core.InMemoryProvider
{
    internal class Stream<T> : IQbservable<T>
    {
        private readonly StreamDbContextOptions _options;
        private readonly IObservable<T> _observable;

        public Type ElementType { get; private set; }
        public Expression Expression { get; private set; }
        public IQbservableProvider Provider { get; private set; }

        // Called first by stream factory given to context options
        public Stream(IObservable<T> observable, StreamDbContextOptions options)
        {
            _observable = observable;
            _options = options;

            ElementType = typeof(T);
            Provider = new StreamDbProvider<T>(observable, options);  // Provider will create stream in constructor below if there's a linq query involved
            Expression = SerializationHelper.NewObserverParameter<T>();
        }

        // Stream is created a second time by qbservable provider when linq is used.
        public Stream(
            IQbservableProvider provider,
            Expression expression,
            IObservable<T> observable,
            StreamDbContextOptions options)
        {
            _observable = observable;
            _options = options;

            ElementType = typeof(T);
            Provider = provider;
            Expression = expression;
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            var sub = new Subscription<T>(Expression, _observable, _options);
            sub.Attach(observer);
            return sub;
        }
    }
}
