namespace Qube.Grpc
{
    public class TestEvent<T>
    {
        public string Id { get; set; }
        public string Category { get; set; }
        public T Body { get; set; }
    }
}