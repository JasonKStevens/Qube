using Qube.Core;
using System;
using System.Linq.Expressions;
using System.Reactive.Linq;

namespace Qube.InMemory
{
    internal class StreamDbProvider<TIn> : IQbservableProvider
    {
        private readonly IObservable<TIn> _observable;
        private readonly StreamDbContextOptions _options;
        
        public StreamDbProvider(IObservable<TIn> observable, StreamDbContextOptions options)
        {
            _observable = observable;
            _options = options;
        }

        public IQbservable<TOut> CreateQuery<TOut>(Expression expression)
        {
            return new QueryStream<TIn, TOut>(this, expression, _observable, _options);
        }
    }
}
