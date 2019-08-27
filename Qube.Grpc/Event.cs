namespace Qube.Grpc
{
    public class Event
    {
        public string Id { get; set; }
        public string Category { get; set; }
        public dynamic Body { get; set; }
    }
}