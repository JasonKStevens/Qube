using System;
using System.Reactive.Linq;

namespace QbservableProvider.Common
{
    public class StreamDbContext
    {
        private readonly string _url;

        public StreamDbContext(string url)
        {
            _url = url;
        }

        public IQbservable<Event> FromAll()
        {
            return new Stream<Event>(_url);
        }

        public IQbservable<Event> FromCategory(string categoryName)
        {
            throw new NotImplementedException();
        }

        public IQbservable<Event> FromCategories(params string[] categoryNames)
        {
            throw new NotImplementedException();
        }

        public IQbservable<Event> FromStream(string streamName)
        {
            return new Stream<Event>(_url, streamName);
        }

        public IQbservable<Event> FromStreams(params string[] streamName)
        {
            throw new NotImplementedException();
        }
    }
}
