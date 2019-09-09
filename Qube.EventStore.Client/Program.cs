using System;
using System.Linq;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Newtonsoft.Json;
using Qube.Core;

namespace Qube.EventStore.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var baseTypes = new[] { typeof(BaseEvent) };
            var eventTypes = typeof(BaseEvent).Assembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(BaseEvent)));
            
            var options = new StreamDbContextOptionsBuilder()
                .UseEventStore("127.0.0.1:5001")
                .RegisterTypes(() => baseTypes.Concat(eventTypes))
                .Options;

            var es = new EventStoreContext(options);
            var orderEvents = es.FromStreams<BaseEvent>("Order-*", "PaymentPlan-*");
            var customerEvents = es.FromStreams<BaseEvent>("Customer-*");

            var stream = orderEvents
                .GroupBy(e =>
                    (e is OrderCreatedEvent) ? ((OrderCreatedEvent)e).Id :
                    (e is OrderCustomerLinkedV2Event) ? ((OrderCustomerLinkedV2Event)e).OrderId :
                    (e is OrderConfirmedEvent) ? ((OrderConfirmedEvent)e).OrderId :
                    (e is PaymentPlanCreatedEvent) ? ((PaymentPlanCreatedEvent)e).Id :
                    Guid.Empty
                 )
                .Where(g => g.Key != Guid.Empty)
                //.Select(g => g
                //    .Aggregate(
                //        Guid.Empty,
                //        (s, e) => (Guid)(e.GetType().GetProperty("CustomerId") == null ? s : e.GetType().GetProperty("CustomerId").GetValue(e, null))
                //    )
                //    .Zip(g, (c, e) => g.Where(x => !x.Bag.ContainsKey("CustomerId")).Do(x => x.Bag.Add("CustomerId", c)))
                //    .SelectMany(x => x)
                //)
                .SelectMany(g => g)
                .Merge(customerEvents)  // NB: This doesn't actually work - it's an observable and there's more to joining streams than this...
                .GroupBy(e =>
                    (e is OrderCustomerLinkedV2Event) ? ((OrderCustomerLinkedV2Event)e).CustomerId :
                    (e is OrderConfirmedEvent) ? ((OrderConfirmedEvent)e).CustomerId :
                    //(e is OrderCreatedEvent) ? (Guid)(e.Bag.ContainsKey("CustomerId") ? e.Bag["CustomerId"] : Guid.Empty) :
                    //(e is PaymentPlanCreatedEvent) ? (Guid)(e.Bag.ContainsKey("CustomerId") ? e.Bag["CustomerId"] : Guid.Empty) :
                    (e is CustomerDetailsAddedEvent) ? ((CustomerDetailsAddedEvent)e).CustomerId :
                    Guid.Empty
                )
                //.Where(g => g.Key == new Guid("879c5d39-fd72-4392-bf88-8bba363e590e"))
                .SelectMany(g => g)
                .Subscribe(
                    onNext: s => Console.WriteLine(JsonConvert.SerializeObject(s)),
                    onError: e => Console.WriteLine("ERROR: " + e),
                    onCompleted: () => Console.WriteLine("DONE")
                );

            while (!Console.KeyAvailable && Console.ReadKey(true).Key != ConsoleKey.Escape)
            {
                await Task.Delay(50);
            }
        }
    }
}
