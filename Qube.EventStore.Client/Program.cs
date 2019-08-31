using System;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Qube.Core;
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

            
            var options = new StreamDbContextOptionsBuilder()
                .UseEventStore("127.0.0.1:5001")
                .Options;

            new EventStoreContext(options)
                .FromAll()
                .Where(e => e.EventType == "CustomerCreatedEvent")
                .Where(e => new DateTime(2018, 3, 1) <= e.Created)
                .TakeWhile(e => e.Created < new DateTime(2018, 4, 1))
                .Select(e => e.Data)
                .Subscribe(
                    onNext: s =>
                    {
                        var @event = JsonConvert.DeserializeObject<CustomerCreatedEvent>(s);
                        Console.WriteLine($"{@event.CustomerId}: {@event.Email}");
                    },
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
