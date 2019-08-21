using System;
using System.Reactive.Linq;
using System.Reflection;

namespace QbservableProvider.Core
{
    public class StreamDbContextOptions
    {
        private Func<StreamDbContextOptions, Type, object> _streamFactory;

        public string ConnectionString { get; private set; }

        internal void SetStreamFactory(Func<StreamDbContextOptions, Type, object> streamFactory)
        {
            _streamFactory = streamFactory;
        }

        internal void SetConnectionString(string url)
        {
            ConnectionString = url;
        }

        internal IQbservable<T> CreateStream<T>()
        {
            return (IQbservable<T>) _streamFactory(this, typeof(T));
        }
    }
}
