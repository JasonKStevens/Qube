using Qube.EventStore;

namespace Qube.Core
{
    public class EventStoreContext : StreamDbContext<Event>
    {
        public EventStoreContext(StreamDbContextOptions options) : base(options)
        {
        }
    }
}
