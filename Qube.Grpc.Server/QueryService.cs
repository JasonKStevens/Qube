using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Newtonsoft.Json;
using Qube.Core;
using Qube.Grpc;
using Qube.Grpc.Utils;

namespace QbservableProvider.Grpc.Server
{
    public class QueryService : StreamService.StreamServiceBase
    {
        private static readonly Random random = new Random();

        private readonly SemaphoreSlim _writeLock = new SemaphoreSlim(1, 1);
        private readonly Subject<Event> _subject = new Subject<Event>();

        public override async Task QueryStreamAsync(
            QueryEnvelope queryEnvelope,
            IServerStreamWriter<EventEnvelope> responseStream,
            ServerCallContext callContext)
        {
            var qbservable = await BuildQbservableAsync(queryEnvelope.Payload, _subject, responseStream);
            var @continue = true;

            var sub = qbservable
                .Subscribe(async e => {
                    var @event = EnvelopeHelper.Pack(e);
                    
                    try
                    {
                        await responseStream.WriteAsync(@event);
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

        private async Task SendNextToClient(
            IServerStreamWriter<EventEnvelope> responseStream,
            object payload)
        {
            var eventEnvelope = new EventEnvelope
            {
                Payload = JsonConvert.SerializeObject(payload)
            };
            await SendEnvelopeToClient(responseStream, eventEnvelope);
        }

        private async Task SendErrorToClient(
            IServerStreamWriter<EventEnvelope> responseStream,
            string errorMessage
        )
        {
            var eventEnvelope = new EventEnvelope { Error = errorMessage };
            await SendEnvelopeToClient(responseStream, eventEnvelope);
        }

        private async Task SendEnvelopeToClient(IServerStreamWriter<EventEnvelope> responseStream, EventEnvelope eventEnvelope)
        {
            // TODO: Consider using a buffer - only one write can be pending at a time.
            await _writeLock.WaitAsync();

            try
            {
                await responseStream.WriteAsync(eventEnvelope);
            }
            finally
            {
                _writeLock.Release();
            }
        }

        private async Task<IQbservable<object>> BuildQbservableAsync(
            string serializedRxQuery,
            Subject<Event> subject,
            IServerStreamWriter<EventEnvelope> responseStream)
        {
            try
            {
                var expression = SerializationHelper.DeserializeLinqExpression(serializedRxQuery);
                var lambdaExpression = (LambdaExpression)expression;

                var castLambdaExpression = CastGenericItemToObject(lambdaExpression);

                var qbservable = castLambdaExpression
                    .Compile()
                    .DynamicInvoke(subject.AsQbservable());

                return (IQbservable<object>)qbservable;
            }
            catch (Exception ex)
            {
                await SendErrorToClient(responseStream, "Unsupported linq expression: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Call .Cast<object>() Rx querie's lambda expression to cast result type to "known" object.
        /// </summary>
        private static LambdaExpression CastGenericItemToObject(LambdaExpression lambdaExpression)
        {
            var castMethod = typeof(Qbservable)
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where(mi => mi.Name == "Cast")
                .Single()
                .MakeGenericMethod(typeof(object));

            var methodCall = Expression.Call(null, castMethod, lambdaExpression.Body);
            var castLambdaExpression = Expression.Lambda(methodCall, lambdaExpression.Parameters);

            return castLambdaExpression;
        }
    }
}
