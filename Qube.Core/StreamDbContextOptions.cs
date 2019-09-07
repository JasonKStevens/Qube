using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Qube.Core
{
    public class StreamDbContextOptions
    {
        private Func<StreamDbContextOptions, Type, string[], object> _streamFactory;

        public string ConnectionString { get; private set; }
        public List<Type> TypesToTransfer { get; private set; } = new List<Type>();

        public void SetStreamFactory(Func<StreamDbContextOptions, Type, string[], object> streamFactory)
        {
            _streamFactory = streamFactory;
        }

        public void SetConnectionString(string url)
        {
            ConnectionString = url;
        }

        public void RegisterTypes(IEnumerable<Type> types)
        {
            TypesToTransfer.AddRange(types);
        }

        public IQbservable<TIn> CreateStream<TIn>(params string[] streamPatterns)
        {
            return (IQbservable<TIn>)_streamFactory(this, typeof(TIn), streamPatterns);
        }
    }
}
