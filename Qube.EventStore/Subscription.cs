using Qube.Core;

namespace Qube.EventStore
{
    public class Subscription : Grpc.Subscription
    {
        public Subscription(StreamDbContextOptions options) : base(options)
        {
        }
    }
}
