# QbservableProvider
A proof-of-concept Qbservable provider for running Reactive Extension (Rx) queries over the wire. [gRPC](https://grpc.io) is used for its bi-directional streaming protocol.

This provider does not transpile linq-to-some-other-language, it's intended for a streaming database written in C#.

Here's an example of a query,

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

The linq expression is serialized using [Serialize.Linq](https://github.com/esskar/Serialize.Linq), sent to the server and executed there.  So only the results are sent back to the client (the Rx observer).

## Why?
IQbservable is to IObservable as IQueryable is to IEnumerable, so it follows that stream databases will look towards impementing IQbservable providers.

I discovered the power of [EventStore](https://github.com/EventStore/EventStore) recently.  Previously a projection in C# would take 4 hours, but with EventStore's built-in map-reduce capability running server-side, it now takes 50 seconds.  But it wasn't just the performance, it was the querying capabilities that got me interested.  I wanted to compare Rx with [EventStore's query API](https://eventstore.org/docs/projections/user-defined-projections/index.html) when I realised it would be straight-forward to implement an IQbservable provider since there's no transpiling.

So this is an exploratory project for me to take a deep-dive into streaming databases, Rx and to find out what some of the real-world limitations are.  I hope to hook this up to a fork of EventStore at some point.

## Limitations
There's no support for anonymous types at the moment which are a must for storing state in the reducers really. The following _doesn't_ work yet,

```c#
new StreamDbContext("https://localhost:5001")
    .FromAll()
    .GroupBy(e => e.Category)
    .SelectMany(x => x.Scan(
        new { Count = 0, Category = "" },
        (s, e) => new { Count = s.Count + 1, e.Category }
    ))
    .Subscribe(s => Console.WriteLine(s.Category + ": " + s.Count));
```

As with IQueryable, dynamic types and statement bodies can't be used because of the expression trees.
