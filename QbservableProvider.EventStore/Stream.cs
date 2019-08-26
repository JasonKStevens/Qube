using LiteGuard;
using QbservableProvider.Core;
using System;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reactive.Linq;

namespace QbservableProvider.EventStore
{
    internal class Stream<T> : IQbservable<T>
    {
        private readonly StreamDbContextOptions _options;

        public Type ElementType { get; private set; }
        public Expression Expression { get; private set; }
        public IQbservableProvider Provider { get; private set; }

        public Stream(StreamDbContextOptions options)
        {
            _options = options;

            ElementType = typeof(T);
            Provider = new StreamDbProvider<T>(options);
            Expression = SerializationHelper.NewObserverParameter<T>();
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            var sub = new Subscription(_options);
            sub.Connect<T, T>(Expression, observer);
            return sub;
        }
    }
}
