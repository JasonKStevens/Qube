using System;
using System.Linq.Expressions;
using System.Reactive.Linq;

namespace QbservableProvider.Core.InMemoryProvider
{
    internal class StreamDbProvider<T> : IQbservableProvider
    {
        private IObservable<T> _observable;
        private StreamDbContextOptions _options;
        
        public StreamDbProvider(IObservable<T> observable, StreamDbContextOptions options)
        {
            _observable = observable;
            _options = options;
        }

        public IQbservable<TResult> CreateQuery<TResult>(Expression expression)
        {
            if (typeof(T) != typeof(TResult))
                throw new InvalidCastException($"Query type {typeof(TResult).Name} must match source observable type {typeof(T).Name}.");
            return (IQbservable<TResult>) new Stream<T>(this, expression, _observable, _options);
        }
    }
}
