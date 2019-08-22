# QbservableProvider
Write [Rx](https://github.com/dotnet/reactive) queries on the client, have them run on the remote server and stream back the results via [gRPC](https://grpc.io).

This library isn't for transpiling _linq-to-some-query-language_, it's intended for stream databases or servers written in C# like [EventStore](https://github.com/EventStore/EventStore). In other words, linq _is_ the query language, along with observables and the built-in power of the Rx schedulers.

```c#
// Simple example
new StreamDbContext("https://localhost:5001")
    .FromAll()
    .Where(e => e.Category == "Category1")
    .Select(e => e.Id)
    .Subscribe(s => Console.WriteLine(s));
```

Linq expressions are serialized using [Serialize.Linq](https://github.com/esskar/Serialize.Linq), sent to the server, wrapped around an observable, and the results are sent back to the client observer.

```c#
// Map-reduce example
new StreamDbContext("https://localhost:5001")
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
The project is currently moving from a proof-of-concept to a maintainable code-base. The design's taking shape and unit tests are taking precedence.

The focus is on the in-memory provider at the moment. Then the generic gRPC provider will be used to to flesh out remote server support, and from there an EventStore provider will be looked at.

There's a client & server project to run that show things basically working.

## Motivation
I discovered the power of EventStore recently.  Previously, a projection written in C# - with many lines of code and supporting infrastructure - would take four hours to run.  But with EventStore's server-side map-reduce, the projection now takes just 50 seconds. And it does this in just a couple of dozen lines of JavaScript.  But it wasn't just the performance and reduction in developer effort that I was impressed with, it was the querying capabilities as well.

So being more familiar with Rx I wanted to compare its capabilities to [EventStore's query API](https://eventstore.org/docs/projections/user-defined-projections/index.html). Then I realised that it would be straight-forward to implement an IQbservable provider since no transpiling is required. So here we are.

This is an exploratory project for me to do a deep-dive into streaming databases. My goals are to learn more about them, Rx and what the real-world limitations are.

## Direction
The intention is to support the following features, which will bring this project towards the capabilities of EventStore's current query API.

### Anonymous Types
Anonymous types are not supported by Serialize.Linq at the moment, which is a must for storing state in the reducers conveniently and it would be handy with results. The following _doesn't_ work yet but is what the query above would look like with anonymous type support,

```c#
new StreamDbContext("https://localhost:5001")
    .FromAll()
    .GroupBy(e => e.Category)
    .SelectMany(x => x.Scan(
        new { Count = 0, Category = null },
        (s, e) => new { Count = s.Count + 1, e.Category }
    ));
```

### Multiple Event Types
As with IQueryable, dynamic types (and statement bodies) can't be used because of the expression trees in the interface.  Event streams can have different event types, so there needs to be a way to query over them without a lot of fuss.

### Server Actions
Server-side actions in the query are important, for example to `emit` and `linkTo` new streams.

### Enumerables
Rather than just observers it would be nice send back (async) enumerations.

### And then...
From here it's really just about seeing what's possible with Rx's schedulers and operations, like `join`, `merge`, `zip`, `fork`, `buffer`, `throttle`, `sample`, etc.

And looking at use-cases for compound from clauses,

```c#
from e1 in db.FromAll()
from e2 in db.FromCategory("Category1")
from e2 in db.FromStreams("StreamA", "StreamB")
where /* etc */
select /* etc */
```
