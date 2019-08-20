using System.Linq.Expressions;
using System.Reactive.Linq;

namespace QbservableProvider.Common
{
    internal class StreamDbProvider : IQbservableProvider
    {
        private readonly string _url;

        public StreamDbProvider(string url)
        {
            _url = url;
        }

        public IQbservable<T> CreateQuery<T>(Expression expression)
        {
            return new Stream<T>(this, expression, _url);
        }
    }
}
