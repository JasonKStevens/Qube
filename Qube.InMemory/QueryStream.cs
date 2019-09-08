using Qube.Core;
using System;
using System.Linq.Expressions;
using System.Reactive.Linq;

namespace Qube.InMemory
{
    internal class QueryStream<TIn, TOut> : IQbservable<TOut>
    {
        private readonly StreamDbContextOptions _options;
        private readonly string[] _streamPatterns;
        private readonly IObservable<TIn> _observable;

        public Type ElementType { get; private set; }
        public Expression Expression { get; private set; }
        public IQbservableProvider Provider { get; private set; }

        public QueryStream(
            IQbservableProvider provider,
            Expression expression,
            IObservable<TIn> observable,
            StreamDbContextOptions options,
            string[] streamPatterns)
        {
            _observable = observable;
            _options = options;
            _streamPatterns = streamPatterns;

            ElementType = typeof(TIn);
            Provider = provider;
            Expression = expression;
        }

        public IDisposable Subscribe(IObserver<TOut> observer)
        {
            var sub = new Subscription<TIn, TOut>(Expression, _observable, _options, _streamPatterns);
            sub.Attach(observer);
            return sub;
        }
    }
}
