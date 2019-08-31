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

        public IQbservable<T> FromAll<T>()
        {
            return _options.CreateStream<T>();
        }

        public IQbservable<T> FromCategory<T>(string categoryName)
        {
            throw new NotImplementedException();
        }

        public IQbservable<T> FromCategories<T>(params string[] categoryNames)
        {
            throw new NotImplementedException();
        }

        public IQbservable<T> FromStream<T>(string streamName)
        {
            throw new NotImplementedException();
        }

        public IQbservable<T> FromStreams<T>(params string[] streamName)
        {
            throw new NotImplementedException();
        }
    }
}
