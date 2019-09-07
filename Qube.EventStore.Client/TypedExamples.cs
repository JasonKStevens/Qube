//using System;
//using System.Collections.Generic;

//namespace Qube.EventStore.Client
//{
//    public class BaseEvent
//    {
//        public Guid EventId { get; set; } = Guid.NewGuid();
//        public int Version { get; set; }
//        public IDictionary<string, object> Bag = new Dictionary<string, object>();
//        public BaseEvent AddToBag(string key, object @object)
//        {
//            if (@object != null)
//            {
//                Bag[key] = @object;
//            }
//            return this;
//        }
//    }

//    public class CustomerCreatedEvent : BaseEvent
//    {
//        public Guid CustomerId { get; set; }
//        public string Email { get; set; }
//        public string PhoneNumber { get; set; }
//        public string FirstName { get; set; }
//        public string LastName { get; set; }
//    }
//}
