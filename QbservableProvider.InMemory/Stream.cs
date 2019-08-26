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

        public Stream(IObservable<T> observable, StreamDbContextOptions options)
        {
            _observable = observable;
            _options = options;

            ElementType = typeof(T);
            Provider = new StreamDbProvider<T>(observable, options);
            Expression = SerializationHelper.NewObserverParameter<T>();
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            var sub = new Subscription<T, T>(Expression, _observable, _options);
            sub.Attach(observer);
            return sub;
        }
    }
}
