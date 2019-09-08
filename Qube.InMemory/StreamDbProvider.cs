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
        private readonly string[] _streamPatterns;

        public StreamDbProvider(IObservable<TIn> observable, StreamDbContextOptions options, string[] streamPatterns)
        {
            _observable = observable;
            _options = options;
            _streamPatterns = streamPatterns;
        }

        public IQbservable<TOut> CreateQuery<TOut>(Expression expression)
        {
            return new QueryStream<TIn, TOut>(this, expression, _observable, _options, _streamPatterns);
        }
    }
}
