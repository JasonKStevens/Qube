using System;
using System.Reactive.Linq;

namespace QbservableProvider.Core
{
    public class StreamDbContext<T>
    {
        private readonly StreamDbContextOptions _options;

        public StreamDbContext(StreamDbContextOptions options)
        {
            _options = options;
        }

        public IQbservable<T> FromAll()
        {
            return _options.CreateStream<T>();
        }

        public IQbservable<T> FromCategory(string categoryName)
        {
            throw new NotImplementedException();
        }

        public IQbservable<T> FromCategories(params string[] categoryNames)
        {
            throw new NotImplementedException();
        }

        public IQbservable<T> FromStream(string streamName)
        {
            throw new NotImplementedException();
        }

        public IQbservable<T> FromStreams(params string[] streamName)
        {
            throw new NotImplementedException();
        }
    }
}
