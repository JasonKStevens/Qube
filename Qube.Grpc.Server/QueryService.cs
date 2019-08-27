using System;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Grpc.Core;
using Qube.Core;
using Qube.Grpc;
using Qube.Grpc.Utils;

namespace QbservableProvider.Grpc.Server
{
    public class QueryService : StreamService.StreamServiceBase
    {
        private static readonly Random random = new Random();

        private readonly Subject<Event> _subject = new Subject<Event>();

        public override async Task QueryStreamAsync(
            QueryEnvelope queryEnvelope,
            IServerStreamWriter<EventEnvelope> streamWriter,
            ServerCallContext callContext)
        {
            var expression = SerializationHelper.DeserializeLinqExpression(queryEnvelope.Payload);
            var lambdaExpr = (LambdaExpression) expression;
            var lambda = lambdaExpr.Compile();
            var @continue = true;

            // TODO: Build this up with reflection so the generic parameter isn't restricted
            var qbservable = (IQbservable<object>) lambda.DynamicInvoke(_subject.AsQbservable());

            var sub = qbservable
                .Subscribe(async e => {
                    var @event = EnvelopeHelper.Pack(e);
                    
                    try
                    {
                        await streamWriter.WriteAsync(@event);
                    }
                    catch (InvalidOperationException)  // TEMP: Quick cheat to determine client has disconnected
                    {
                        @continue = false;
                    }
                });

            // TEMP: Simulate stream
            while (@continue)
            {
                var @event = new Event
                {
                    Id = random.Next(0, 10).ToString(),
                    Category = "Category" + random.Next(0, 3).ToString(),
                    Body = new { SomeProp = "test" }
                };

                _subject.OnNext(@event);
                await Task.Delay(random.Next(0, 1000));
            }

            sub.Dispose();
        }
    }
 }
