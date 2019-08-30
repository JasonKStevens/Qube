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
            IServerStreamWriter<ResponseEnvelope> responseStream,
            ServerCallContext callContext)
        {
            var qbservable = await BuildQbservableAsync(queryEnvelope.Payload, _subject, responseStream);
            var done = false;

            using (var sub = qbservable.Subscribe(
                async e => await RespondNext(responseStream, e),
                async ex => await RespondError(responseStream, ex.Message),
                async () => await RespondCompleted(responseStream)))
            {
                try
                {
                    while (!done)
                    {
                        var @event = new Event
                        {
                            Id = random.Next(0, 10).ToString(),
                            Category = "Category" + random.Next(0, 3).ToString(),
                            Body = new { SomeProp = "test" }
                        };
                        _subject.OnNext(@event);

                        await Task.Delay(random.Next(0, 500));
                        done = random.Next(0, 20) == 0;
                    }
                }
                catch (Exception ex)
                {
                    _subject.OnError(ex);
                    return;
                }

                _subject.OnCompleted();
            }
        }

        private async Task RespondNext(
            IServerStreamWriter<ResponseEnvelope> responseStream,
            object payload)
        {
            var responseEnvelope = new ResponseEnvelope
            {
                Payload = EnvelopeHelper.Pack(payload),
                ResponseType = ResponseEnvelope.Types.ResponseType.Next
            };
            await SendEnvelopeToClient(responseStream, responseEnvelope);
        }

        private async Task RespondCompleted(IServerStreamWriter<ResponseEnvelope> responseStream)
        {
            var responseEnvelope = new ResponseEnvelope
            {
                Payload = "",
                ResponseType = ResponseEnvelope.Types.ResponseType.Completed
            };
            await SendEnvelopeToClient(responseStream, responseEnvelope);
        }

        private async Task RespondError(
            IServerStreamWriter<ResponseEnvelope> responseStream,
            string errorMessage)
        {
            var responseEnvelope = new ResponseEnvelope
            {
                Payload = errorMessage,
                ResponseType = ResponseEnvelope.Types.ResponseType.Error
            };
            await SendEnvelopeToClient(responseStream, responseEnvelope);
        }

        private async Task SendEnvelopeToClient(IServerStreamWriter<ResponseEnvelope> responseStream, ResponseEnvelope ResponseEnvelope)
        {
            // TODO: Consider using a buffer - only one write can be pending at a time.
            await _writeLock.WaitAsync();

            try
            {
                await responseStream.WriteAsync(ResponseEnvelope);
            }
            finally
            {
                _writeLock.Release();
            }
        }

        private async Task<IQbservable<object>> BuildQbservableAsync(
            string serializedRxQuery,
            Subject<Event> subject,
            IServerStreamWriter<ResponseEnvelope> responseStream)
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
                await RespondError(responseStream, "Unsupported linq expression: " + ex.Message);
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
