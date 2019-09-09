using System;
using System.Collections.Generic;

namespace Qube.EventStore
{
    public class BaseEvent
    {
        public Guid EventId { get; set; } = Guid.NewGuid();
        public int Version { get; set; }
        public Dictionary<string, object> Bag = new Dictionary<string, object>();
    }

    public class OrderCreatedEvent : BaseEvent
    {
        public Guid Id { get; set; }
        public DateTimeOffset CreatedDateTime { get; set; }
        public string Email { get; set; }
        public string FirstNames { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public decimal? Amount { get; set; }
    }

    public class OrderCustomerLinkedV2Event : BaseEvent
    {
        public Guid CustomerId { get; set; }
        public Guid OrderId { get; set; }
    }

    public class OrderConfirmedEvent : BaseEvent
    {
        public Guid OrderId { get; set; }
        public Guid CustomerId { get; set; }
    }

    public class PaymentPlanCreatedEvent : BaseEvent
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime StartDate { get; set; }
        public decimal? InterestRate { get; set; }
    }

    public class CustomerCreatedEvent : BaseEvent
    {
        public Guid CustomerId { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class CustomerDetailsAddedEvent : BaseEvent
    {
        public Guid? OrderId { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleNames { get; set; }
    }
}
