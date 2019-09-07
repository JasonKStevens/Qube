using System;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Qube.Core;
using PartPay.Common.Domain.Purchase;
using PartPay.Common.Domain.Payment;
using System.Reflection;
using System.Linq;
using PartPay.Common;
using Newtonsoft.Json;

namespace Qube.EventStore.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // TODO: Support anonymous types for this query
            //new StreamDbContext("https://127.0.0.1:5001")
            //    .FromAll()
            //    .GroupBy(e => e.Category)
            //    .SelectMany(x => x.Scan(new { Count = 0, Category = "" }, (s, e) => new { Count = s.Count + 1, e.Category }))
            //    .Subscribe(s => Console.WriteLine(s.Category + ": " + s.Count));

            // TODO: Support anonymous types for this query (go figure)
            //(
            //    from e1 in new StreamDbContext("https://127.0.0.1:5001").FromAll()
            //    from e2 in new StreamDbContext("https://127.0.0.1:5001").FromAll()
            //    where e1.Category == e1.Category
            //    select e1.Id + " - " + e2.Id
            //).Subscribe(e => Console.WriteLine(e));

            //.GroupBy(e => e.EventStreamId.Split('-')[0])
            //.SelectMany(g =>
            //    g.Scan(
            //        $"{g.Key}:0",
            //        (s, e) => $"{e.EventStreamId}:{int.Parse(s.Split(':')[1]) + 1}"
            //    )
            //)

            var types = Assembly.Load("PartPay.Common");
            var baseTypes = new [] { typeof(BaseEvent) };
            //var enumTypes = types.GetTypes().Where(t => t.IsEnum);
            var eventTypes = types.GetTypes().Where(t => t.IsSubclassOf(typeof(BaseEvent)));

            var options = new StreamDbContextOptionsBuilder()
                .UseEventStore("127.0.0.1:5001")
                .RegisterTypes(() => baseTypes.Concat(eventTypes))
                .Options;

            var es = new EventStoreContext(options);

            var orderEvents = es.FromAll<BaseEvent>();
            var customerEvents = es.FromAll<BaseEvent>();

            var stream = orderEvents
                .GroupBy(e =>
                    (e is OrderCreatedEvent) ? ((OrderCreatedEvent)e).Id :
                    (e is OrderCustomerLinkedEvent) ? ((OrderCustomerLinkedEvent)e).OrderId :
                    (e is OrderCustomerLinkedV2Event) ? ((OrderCustomerLinkedV2Event)e).OrderId :
                    (e is OrderConfirmedEvent) ? ((OrderConfirmedEvent)e).OrderId :
                    (e is OrderRefundedEvent) ? ((OrderRefundedEvent)e).OrderId :
                    (e is OrderAbandondedEvent) ? ((OrderAbandondedEvent)e).OrderId :
                    (e is PaymentPlanCreatedEvent) ? ((PaymentPlanCreatedEvent)e).Id :
                    (e is InstallmentPaymentApprovedEvent) ? ((InstallmentPaymentApprovedEvent)e).PaymentPlanId :
                    (e is ManualPaymentApprovedEvent) ? ((ManualPaymentApprovedEvent)e).PaymentPlanId :
                    (e is BankTransferApprovedEvent) ? ((BankTransferApprovedEvent)e).PaymentPlanId :
                    (e is OutstandingInstallmentBalancePaymentApprovedEvent) ? ((OutstandingInstallmentBalancePaymentApprovedEvent)e).PaymentPlanId :
                    (e is InstallmentDefaultFeeRaisedEvent) ? ((InstallmentDefaultFeeRaisedEvent)e).PaymentPlanId :
                    (e is InstallmentInArrearsFeeRaisedEvent) ? ((InstallmentInArrearsFeeRaisedEvent)e).PaymentPlanId :
                    (e is PaymentPlanPaidOffEvent) ? ((PaymentPlanPaidOffEvent)e).PaymentPlanId :
                    (e is InstallmentPaymentDeclinedEvent) ? ((InstallmentPaymentDeclinedEvent)e).PaymentPlanId :
                    (e is PaymentPlanInstallmentsUpdatedEvent) ? ((PaymentPlanInstallmentsUpdatedEvent)e).Id :
                    Guid.Empty
                 )
                .Where(g => g.Key != Guid.Empty)
                .Select(g => g
                    .Aggregate(
                        Guid.Empty,
                        (s, e) => (Guid)(e.GetType().GetProperty("CustomerId") == null ? s : e.GetType().GetProperty("CustomerId").GetValue(e, null))
                    )
                    .Zip(g, (c, e) => g.Where(x => !x.Bag.ContainsKey("CustomerId")).Do(x => x.Bag.Add("CustomerId", c)))
                    .SelectMany(x => x)
                )
                .SelectMany(g => g)
                .Merge(customerEvents)
                .GroupBy(e =>
                    (e is OrderCustomerLinkedEvent) ? ((OrderCustomerLinkedEvent)e).CustomerId :
                    (e is OrderCustomerLinkedV2Event) ? ((OrderCustomerLinkedV2Event)e).CustomerId :
                    (e is OrderConfirmedEvent) ? ((OrderConfirmedEvent)e).CustomerId :
                    (e is OrderRefundedEvent) ? ((OrderRefundedEvent)e).CustomerId :
                    (e is OrderCreatedEvent) ? (Guid)(e.Bag.ContainsKey("CustomerId") ? e.Bag["CustomerId"] : Guid.Empty) :
                    (e is OrderAbandondedEvent) ? (Guid)(e.Bag.ContainsKey("CustomerId") ? e.Bag["CustomerId"] : Guid.Empty) :
                    (e is PaymentPlanCreatedEvent) ? (Guid)(e.Bag.ContainsKey("CustomerId") ? e.Bag["CustomerId"] : Guid.Empty) :
                    (e is InstallmentPaymentApprovedEvent) ? (Guid)(e.Bag.ContainsKey("CustomerId") ? e.Bag["CustomerId"] : Guid.Empty) :
                    (e is ManualPaymentApprovedEvent) ? (Guid)(e.Bag.ContainsKey("CustomerId") ? e.Bag["CustomerId"] : Guid.Empty) :
                    (e is BankTransferApprovedEvent) ? (Guid)(e.Bag.ContainsKey("CustomerId") ? e.Bag["CustomerId"] : Guid.Empty) :
                    (e is OutstandingInstallmentBalancePaymentApprovedEvent) ? (Guid)(e.Bag.ContainsKey("CustomerId") ? e.Bag["CustomerId"] : Guid.Empty) :
                    (e is InstallmentDefaultFeeRaisedEvent) ? (Guid)(e.Bag.ContainsKey("CustomerId") ? e.Bag["CustomerId"] : Guid.Empty) :
                    (e is InstallmentInArrearsFeeRaisedEvent) ? (Guid)(e.Bag.ContainsKey("CustomerId") ? e.Bag["CustomerId"] : Guid.Empty) :
                    (e is PaymentPlanPaidOffEvent) ? (Guid)(e.Bag.ContainsKey("CustomerId") ? e.Bag["CustomerId"] : Guid.Empty) :
                    (e is InstallmentPaymentDeclinedEvent) ? (Guid)(e.Bag.ContainsKey("CustomerId") ? e.Bag["CustomerId"] : Guid.Empty) :
                    (e is PaymentPlanInstallmentsUpdatedEvent) ? (Guid)(e.Bag.ContainsKey("CustomerId") ? e.Bag["CustomerId"] : Guid.Empty) :
                    (e is PaymentCardUniquenessSetEvent) ? ((PaymentCardUniquenessSetEvent)e).CustomerId :
                    (e is PaymentCardSetGoodEvent) ? ((PaymentCardSetGoodEvent)e).CustomerId :
                    (e is BureauCreditEnquiryEvent) ? ((BureauCreditEnquiryEvent)e).CustomerId :
                    (e is BureauCreditEnquiryFailedEvent) ? ((BureauCreditEnquiryFailedEvent)e).CustomerId :
                    (e is BureauRecordUniqueCheckedEvent) ? ((BureauRecordUniqueCheckedEvent)e).CustomerId :
                    (e is DriversLicenceUniqueCheckedEvent) ? ((DriversLicenceUniqueCheckedEvent)e).CustomerId :
                    (e is CustomerIdentificationLevelChangedEvent) ? ((CustomerIdentificationLevelChangedEvent)e).CustomerId :
                    (e is CustomerIdentityFailedEvent) ? ((CustomerIdentityFailedEvent)e).CustomerId :
                    (e is CustomerCreditBlockedEvent) ? ((CustomerCreditBlockedEvent)e).CustomerId :
                    (e is CustomerCreditUnblockedEvent) ? ((CustomerCreditUnblockedEvent)e).CustomerId :
                    (e is CustomerMarkedForProfileUpdateEvent) ? ((CustomerMarkedForProfileUpdateEvent)e).CustomerId :
                    (e is CustomerDriverLicenceAddedEvent) ? ((CustomerDriverLicenceAddedEvent)e).CustomerId :
                    (e is CustomerDetailsAddedEvent) ? ((CustomerDetailsAddedEvent)e).CustomerId :
                    (e is CustomerDuplicateDriverLicenceMadeGoodEvent) ? ((CustomerDuplicateDriverLicenceMadeGoodEvent)e).CustomerId :
                    Guid.Empty
                )
                .Where(g => g.Key == new Guid("879c5d39-fd72-4392-bf88-8bba363e590e"))
                .SelectMany(g => g)
                .Subscribe(
                    onNext: s => Console.WriteLine(JsonConvert.SerializeObject(s)),
                    onError: e => Console.WriteLine("ERROR: " + e),
                    onCompleted: () => Console.WriteLine("DONE")
                );



            //new EventStoreContext(options)
            //    .FromAll()
            //    .GroupBy(e => e.EventStreamId.Split(new char[] { '-' })[0])
            //    .SelectMany(g =>
            //        g.Scan(
            //            $"{g.Key}:0",
            //            (s, e) => $"{e.EventStreamId.Split(new char[] { '-' })[0]}:{int.Parse(s.Split(new char[] { ':' })[1]) + 1}"
            //        )
            //    )
            //    .Subscribe(
            //        onNext: s => Console.WriteLine(s),
            //        onError: e => Console.WriteLine("ERROR: " + e),
            //        onCompleted: () => Console.WriteLine("DONE")
            //    );

            while (!Console.KeyAvailable && Console.ReadKey(true).Key != ConsoleKey.Escape)
            {
                await Task.Delay(50);
            }
        }
    }
}
