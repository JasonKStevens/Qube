using EventStore.Transport.Grpc;

namespace Qube.Core
{
    public class EventStoreContext : StreamDbContext<GrpcEvent>
    {
        public EventStoreContext(StreamDbContextOptions options) : base(options)
        {
        }
    }
}
