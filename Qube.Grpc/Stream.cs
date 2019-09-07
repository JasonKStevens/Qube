using Qube.Core;
using System;
using System.Linq.Expressions;
using System.Reactive.Linq;

namespace Qube.Grpc
{
    public class Stream<T> : IQbservable<T>
    {
        private readonly StreamDbContextOptions _options;
        private readonly string[] _streamPatterns;

        public Type ElementType { get; private set; }
        public Expression Expression { get; private set; }
        public IQbservableProvider Provider { get; private set; }

        public Stream(StreamDbContextOptions options, string[] streamPatterns)
        {
            _options = options;
            _streamPatterns = streamPatterns;

            ElementType = typeof(T);
            Provider = new StreamDbProvider<T>(options, _streamPatterns);
            Expression = Expression.Parameter(typeof(IQbservable<T>), "o");
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            var sub = new Subscription(_options, _streamPatterns);
            sub.Connect<T, T>(Expression, observer);
            return sub;
        }
    }
}
