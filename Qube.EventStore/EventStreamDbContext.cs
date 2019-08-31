using Qube.EventStore;

namespace Qube.Core
{
    public class EventStoreContext : StreamDbContext
    {
        public EventStoreContext(StreamDbContextOptions options) : base(options)
        {
        }
    }
}
