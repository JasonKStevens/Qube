using LiteGuard;
using System;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reactive.Linq;

namespace QbservableProvider.Core.GRpcProvider
{
    internal class QueryStream<TIn, TOut> : IQbservable<TOut>
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly StreamDbContextOptions _options;

        public Type ElementType { get; private set; }
        public Expression Expression { get; private set; }
        public IQbservableProvider Provider { get; private set; }

        public QueryStream(
            IQbservableProvider provider,
            Expression expression,
            IHttpClientFactory httpClientFactory,
            StreamDbContextOptions options)
        {
            ElementType = typeof(TIn);
            Provider = provider;
            Expression = expression;
            _httpClientFactory = httpClientFactory;
            _options = options;
        }

        public IDisposable Subscribe(IObserver<TOut> observer)
        {
            var sub = new Subscription(_httpClientFactory, _options);
            sub.Connect<TIn, TOut>(Expression, observer);
            return sub;
        }
    }
}
