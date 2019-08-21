using System;
using System.Linq.Expressions;

namespace QbservableProvider.Core.InMemoryProvider
{
    internal class Subscription<T> : IDisposable
    {
        private readonly Expression _expression;
        private readonly IObservable<T> _observable;
        private readonly StreamDbContextOptions _options;
        private IDisposable _sub;

        internal Subscription(Expression expression, IObservable<T> observable, StreamDbContextOptions options)
        {
            _expression = expression;
            _observable = observable;
            _options = options;
        }

        internal void Attach(IObserver<T> observer)
        {
            _sub = _observable.Subscribe(observer);
        }

        public void Dispose()
        {
            _sub?.Dispose();
        }
    }
}
