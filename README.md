# QbservableProvider (Rx for EventStore)
[![Build Status](https://dev.azure.com/jasonkstevens/PuzzleBox/_apis/build/status/JasonKStevens.QbservableProvider?branchName=master)](https://dev.azure.com/jasonkstevens/PuzzleBox/_build/latest?definitionId=7&branchName=master)

Just as IQueriable was a big deal for relational databases access, it follows that IQbservable will be a big deal for stream or functional databases.

This library lets [Rx](https://github.com/dotnet/reactive) queries written on the client (C#) be executed on the server and so just the results streamed back.

It is an experimental client for [EventStore](https://github.com/EventStore/EventStore) but written to be more general purpose.

```c#
// Simple example
var options = new StreamDbContextOptionsBuilder()
    .UseGRpcStream("https://localhost:5001")
    .Options;

new StreamDbContext(options)
    .FromAll()
    .Where(e => e.Category == "Category1" || e.Category == "Category2")
    .Take(50)
    .Subscribe(s => Console.WriteLine(s.Id));
```

Linq expressions are serialized using [Serialize.Linq](https://github.com/esskar/Serialize.Linq), sent to the server, wrapped around an observable there, and finally the results are streamed back to the client observer.

This library isn't for transpiling _linq-to-some-query-language_, it's intended for stream databases or servers written in C# like [EventStore](https://github.com/EventStore/EventStore). In other words, linq _is_ the query language, along with observables and the built-in power of the Rx schedulers.

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
The project is currently moving from a proof-of-concept to a maintainable code-base. The design's taking shape and unit tests are taking precedence.

The in-memory provider is good enough for the moment so the focus has moved to the gRPC provider which will be used to to flesh out the design for remote server support. From there an EventStore provider will be looked at.

There's a client & server project to run that shows things basically working.

## Motivation
I discovered the power of EventStore recently.  Previously, a projection written in C# - with many lines of code and supporting infrastructure - would take four hours to run.  But with EventStore's server-side map-reduce, the projection now takes just 50 seconds. And it does this in a couple of dozen lines of JavaScript.  But it wasn't just the performance and reduction in developer effort that I was impressed with, it was the querying capabilities as well.

Being more familiar with Rx I wanted to compare it to [EventStore's query API](https://eventstore.org/docs/projections/user-defined-projections/index.html). Then I realised that it would be straight-forward to implement an IQbservable provider for EventStore since no transpiling is required. So here we are.

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
