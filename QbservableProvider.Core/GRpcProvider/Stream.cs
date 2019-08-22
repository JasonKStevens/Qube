using LiteGuard;
using System;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reactive.Linq;

namespace QbservableProvider.Core.GRpcProvider
{
    internal class Stream<T> : IQbservable<T>
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly StreamDbContextOptions _options;

        public Type ElementType { get; private set; }
        public Expression Expression { get; private set; }
        public IQbservableProvider Provider { get; private set; }

        public Stream(IHttpClientFactory httpClientFactory, StreamDbContextOptions options)
        {
            _httpClientFactory = httpClientFactory;
            _options = options;

            ElementType = typeof(T);
            Provider = new StreamDbProvider<T>(httpClientFactory, options);
            Expression = SerializationHelper.NewObserverParameter<T>();
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            var sub = new Subscription(_httpClientFactory, _options);
            sub.Connect<T, T>(Expression, observer);
            return sub;
        }
    }
}
