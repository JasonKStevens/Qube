using System;
using System.Linq.Expressions;
using System.Reactive.Linq;

namespace QbservableProvider.Common
{
    internal class Stream<T> : IQbservable<T>
    {
        private string _url;
        private string _streamName;

        public Type ElementType { get; private set; }
        public Expression Expression { get; private set; }
        public IQbservableProvider Provider { get; private set; }

        public Stream(string url, string streamName = null)
        {
            Provider = new StreamDbProvider(url);
            Expression = SerializationHelper.NewObserverParameter();
            _streamName = streamName;
        }

        public Stream(IQbservableProvider provider, Expression expression, string url)
        {
            ElementType = typeof(T);
            Provider = provider;
            Expression = expression;
            _url = url;
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            var sub = new Subscription(_url, Expression);
            sub.Subscribe(observer);
            return sub;
        }
    }
}
