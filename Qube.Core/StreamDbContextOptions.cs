using System;
using System.Reactive.Linq;
using Qube.Core.Utils;

namespace Qube.Core
{
    public class StreamDbContextOptions
    {
        private Func<StreamDbContextOptions, Type, object> _streamFactory;

        public string ConnectionString { get; private set; }

        public void SetStreamFactory(Func<StreamDbContextOptions, Type, object> streamFactory)
        {
            _streamFactory = streamFactory;
        }

        public void SetConnectionString(string url)
        {
            ConnectionString = url;
        }

        public IQbservable<TIn> CreateStream<TIn>()
        {
            return (IQbservable<TIn>)_streamFactory(this, typeof(TIn));
        }
    }
}
