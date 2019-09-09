using System;

namespace Qube.Grpc.Client
{
    public class TestEvent
    {
        public Guid CustomerId { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
}