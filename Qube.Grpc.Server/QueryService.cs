using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using QbservableProvider.Core;
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
            var queryExpression = SerializationHelper.DeserializeLinqExpression(queryEnvelope.Payload);
            var qbservable = new ServerQueryObservable<Event, object>(_subject.AsQbservable(), queryExpression);

            var isComplete = false;
            var isError = false;
            var eventCount = 0;

            using (var sub = qbservable.Subscribe(
                async e => await ClientOnNext(responseStream, e),
                async ex => await ClientOnError(responseStream, ex),
                async () => await ClientOnCompleted(responseStream)))
            {
                do
                {
                    _subject.OnNext(new Event
                    {
                        Id = random.Next(0, 10).ToString(),
                        Category = "Category" + random.Next(0, 3).ToString(),
                        Body = new { SomeProp = "test" }
                    });

                    await Task.Delay(random.Next(0, 500));

                    if (eventCount++ > 5)
                    {
                        isComplete = random.Next(0, 20) == 0;
                        isError = !isComplete && random.Next(0, 20) == 0;
                    }
                } while (!isComplete && !isError);

                if (isComplete)
                {
                    _subject.OnCompleted();
                }
                else if (isError)
                {
                    _subject.OnError(new Exception("Example error"));
                }

            }
        }

        private async Task ClientOnNext(
            IServerStreamWriter<ResponseEnvelope> responseStream,
            object payload)
        {
            await SendEnvelopeToClient(responseStream, new ResponseEnvelope
            {
                Payload = EnvelopeHelper.Pack(payload),
                ResponseType = ResponseEnvelope.Types.ResponseType.Next
            });
        }

        private async Task ClientOnCompleted(IServerStreamWriter<ResponseEnvelope> responseStream)
        {
            await SendEnvelopeToClient(responseStream, new ResponseEnvelope
            {
                Payload = "",
                ResponseType = ResponseEnvelope.Types.ResponseType.Completed
            });
        }

        private async Task ClientOnError(
            IServerStreamWriter<ResponseEnvelope> responseStream,
            Exception ex)
        {
            await SendEnvelopeToClient(responseStream, new ResponseEnvelope
            {
                Payload = EnvelopeHelper.Pack(ex),
                ResponseType = ResponseEnvelope.Types.ResponseType.Error
            });
        }

        private async Task SendEnvelopeToClient(
            IServerStreamWriter<ResponseEnvelope> responseStream,
            ResponseEnvelope ResponseEnvelope)
        {
            // gRpc - only one write can be pending at a time.
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
    }
}
