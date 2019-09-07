using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Qube.Core
{
    public class StreamDbContextOptions
    {
        private Func<StreamDbContextOptions, Type, object> _streamFactory;

        public string ConnectionString { get; private set; }
        public List<Type> TypesToTransfer { get; private set; } = new List<Type>();

        public void SetStreamFactory(Func<StreamDbContextOptions, Type, object> streamFactory)
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

        public IQbservable<TIn> CreateStream<TIn>()
        {
            return (IQbservable<TIn>)_streamFactory(this, typeof(TIn));
        }

        public IQbservable<TIn> CreateCategoriesStream<TIn>(params string[] categoryNames)
        {
            return (IQbservable<TIn>)_streamFactory(this, typeof(TIn));
        }
    }
}
