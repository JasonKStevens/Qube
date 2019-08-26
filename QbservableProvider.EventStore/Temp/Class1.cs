using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QbservableProvider.EventStore.Client.Temp
{
    public interface IDomainEvent
    {
        Guid EventId { get; }
        int Version { get; }
    }
    [Serializable]
    public class BaseEvent : IDomainEvent
    {
        protected BaseEvent()
        {
            EventId = Guid.NewGuid();
        }

        public Guid EventId { get; set; }
        public int Version { get; set; }
    }
    public class CustomerCreatedEvent : BaseEvent
    {
        public Guid CustomerId { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string CreationSource { get; set; }
    }
}
