using EventStore.Transport.Grpc;

namespace QbservableProvider.Core
{
    public class EventStoreContext : StreamDbContext<GrpcEvent>
    {
        public EventStoreContext(StreamDbContextOptions options) : base(options)
        {
        }
    }
}
