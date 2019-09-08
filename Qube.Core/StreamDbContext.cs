using System;
using System.Reactive.Linq;

namespace Qube.Core
{
    public class StreamDbContext
    {
        private readonly StreamDbContextOptions _options;

        public StreamDbContext(StreamDbContextOptions options)
        {
            _options = options;
        }

        public IQbservable<object> FromAll()
        {
            return FromAll<object>();
        }

        public IQbservable<T> FromAll<T>()
        {
            return _options.CreateStream<T>();
        }

        public IQbservable<T> FromStreams<T>(params string[] streamPatterns)
        {
            return _options.CreateStream<T>(streamPatterns);
        }
    }
}
