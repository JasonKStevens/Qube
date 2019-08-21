namespace QbservableProvider.Core
{
    public class EventStreamDbContext : StreamDbContext<Event>
    {
        public EventStreamDbContext(StreamDbContextOptions options) : base(options)
        {
        }
    }
}
