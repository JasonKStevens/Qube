# QbservableProvider
A serverless Qbservable provider for running [Reactive Extension (Rx)](https://github.com/dotnet/reactive) queries over [gRPC](https://grpc.io).

This provider does not transpile linq-to-some-query-language, it's intended for a streaming database written in C# like [EventStore](https://github.com/EventStore/EventStore). In other words, linq _is_ the query language, along with observables and the built-in Rx schedulers.

```c#
// Simple example
new StreamDbContext("https://localhost:5001")
    .FromAll()
    .Where(e => e.Category == "Category1")
    .Select(e => e.Id)
    .Subscribe(s => Console.WriteLine(s));
```

Linq expressions are serialized using [Serialize.Linq](https://github.com/esskar/Serialize.Linq), then they're sent to the server and executed there.  So only the results are sent back to the client.

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

## Purpose
IQbservable is to IObservable as IQueryable is to IEnumerable, so it follows that stream databases will increasingly support an IQbservable provider.

I discovered the power of EventStore recently.  Previously, a projection written in C# (with many lines of code and supporting infrastructure) would take four hours to run.  But with EventStore's built-in map-reduce running server-side, in a couple of dozen lines of JavaScript it now takes 50 seconds.  But it wasn't just the performance and reduction in developer effort that I was impressed with, it was the querying capabilities as well.

So being more familiar with Rx I wanted to compare its capabilities to [EventStore's query API](https://eventstore.org/docs/projections/user-defined-projections/index.html). But then I realised that it would be straight-forward to implement an IQbservable provider since there's no transpiling required. So here we are.

This is an exploratory project for me to do a deep-dive into streaming databases. My goals are to learn more about them, Rx and what the real-world limitations are.

## Status
The project is currently moving from a proof-of-concept to a more maintainable code-base, so there's some design going in and unit tests are taking precedence.

At the moment the focus is on the in-memory provider to flesh out the design. From there the gRPC provider to flesh out server support, and from there an EventStore provider.

CI will come.

## Direction
The following features are intended, which will bring this project towards the capabilities of EventStore's current query API.

### EventStore Integration
There's little value to this project until it can be wired up to a server - whether it's a stream database or thin, server-side wrapper for something. The plan is to fork EventStore, hook it up and allow some real-world scenarios to be worked through to improve the provider.

### Anonymous Types
Anonymous types are not supported by Serialize.Linq at the moment, which is a must for storing state in the reducers really and would be handy with results. The following _doesn't_ work yet but is what the query above would look like with anonymous type support,

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
As with IQueryable, dynamic types (and statement bodies) can't be used because of expression trees.  In general, this isn't a major but because event streams can have different event types, there does need to be a sensible way to query over them without a lot of fuss.

### Server Actions
Server-side actions in the query are important, for example to `emit` and `linkTo` new streams.

### Enumerables
Rather than just observers it would be nice send back enumerations.

### Beyond
From here it's really just about seeing what's possible with Rx's schedulers and its many operations, like `join`, `merge`, `zip`, `fork`, `buffer`, `throttle`, `sample`, etc.

And looking at use-cases for compound from clauses,

```c#
from e1 in db.FromAll()
from e2 in db.FromCategory("Category1")
from e2 in db.FromStreams("StreamA", "StreamB")
where /* etc */
select /* etc */
```
