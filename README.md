# Qbservable Provider (Rx for EventStore)
[![Build Status](https://dev.azure.com/jasonkstevens/PuzzleBox/_apis/build/status/JasonKStevens.QbservableProvider?branchName=master)](https://dev.azure.com/jasonkstevens/PuzzleBox/_build/latest?definitionId=7&branchName=master)

This library is an experimental [Rx](https://github.com/dotnet/reactive) client for [EventStore](https://github.com/EventStore/EventStore)  written to be general purpose.

Just as IQueriable was a big deal for relational databases access, it follows that IQbservable will be a big deal for stream and functional databases.

This library lets Rx queries be written on the client (C#), executed on the server and just the results streamed back.

```c#
var options = new StreamDbContextOptionsBuilder()
    .UseEventStore("127.0.0.1:5001")
    .Options;

new EventStoreContext(options)
    .FromAll()
    .Where(e => e.EventType == "CustomerCreatedEvent")
    .Where(e => e.Created >= new DateTime(2018, 10, 1))
    .TakeWhile(e => e.Created < new DateTime(2018, 11, 1))
    .Subscribe(
        onNext: s =>
        {
            var @event = JsonConvert.DeserializeObject<CustomerCreatedEvent>(s.Data);
            Console.WriteLine($"{@event.CustomerId}: {@event.Email}");
        },
        onError: e => Console.WriteLine("ERROR: " + e),
        onCompleted: () => Console.WriteLine("DONE")
    );
```

There's no transpiling of _linq-to-some-query-language_ - it's intended for stream databases or servers written in C# like EventStore. In other words, linq _is_ the query language, along with observables and the built-in power of the Rx schedulers.

Linq expressions are serialized using [Serialize.Linq](https://github.com/esskar/Serialize.Linq), sent to the server, wrapped around a subject there, and finally the results are streamed back to the client observer over [gRpc](https://grpc.io/).

```c#
// Map-reduce example
new StreamDbContext(options)
    .FromAll()
    .GroupBy(e => e.Category)
    .SelectMany(g =>
        g.Scan(
            // Anonymous type support will simplify this (see below)
            $"{g.Key}:0",
            (s, e) => $"{e.Category}:{int.Parse(s.Split(':')[1]) + 1}"
        )   
    )
    .Subscribe(
        onNext: s => Console.WriteLine(s),
        onError: e => Console.WriteLine("ERROR: " + e),
        onCompleted: () => Console.WriteLine("DONE")
    );
```

## Status
The project is currently moving from a proof-of-concept to a more maintainable state.

## Motivation
I discovered the power of EventStore's projections recently.  In particular, its server-side map-reduce.  Out of curiosity I wanted to compare [EventStore's query API](https://eventstore.org/docs/projections/user-defined-projections/index.html) with Rx. It was then that I realised that since EventStore was written in C#, it would be fairly straight-forward to implement an IQbservable provider for it. So here we are.

This is an exploratory project for me to do a deep-dive into streaming databases. My goals are to learn more about them, Rx and what the real-world limitations are.

## Direction
The intention is to support the following features, which will bring this project towards the capabilities of EventStore's query API.

### Anonymous Types
Anonymous types are not supported by Serialize.Linq at the moment, which is a must for storing state in the reducers conveniently and it would be handy for results. The following _doesn't_ work yet but is what the query above would look like with anonymous type support,

```c#
new StreamDbContext(options)
    .FromAll()
    .GroupBy(e => e.Category)
    .SelectMany(x => x.Scan(
        new { Count = 0, Category = null },
        (s, e) => new { Count = s.Count + 1, e.Category }
    ));
```

### Multiple Event Types
As with IQueryable, dynamic types (and statement bodies) can't be used because of the expression trees in the interface.  Event streams can have different event types though, so there needs to be a way to query over their properties without a lot of fuss.

### Server Actions
Server-side actions in the query are important, for example to `emit` and `linkTo` new streams.

### Enumerables
Rather than just observers it would be nice send back (async) enumerations.

### From there...
It's really just about seeing what's possible with Rx's schedulers and operations, like `join`, `merge`, `zip`, `fork`, `buffer`, `throttle`, `sample`, etc.

And looking at use-cases for compound from clauses,

```c#
from e1 in db.FromAll()
from e2 in db.FromCategory("Category1")
from e2 in db.FromStreams("StreamA", "StreamB")
where /* etc */
select /* etc */
```

... and hopefully unlock some stream database goodness.
