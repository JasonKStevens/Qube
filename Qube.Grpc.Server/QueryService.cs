using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Qube.Grpc.Utils;

namespace Qube.Grpc.Server
{
    public class QueryService : StreamService.StreamServiceBase
    {
        private static readonly Random random = new Random();

        private readonly SemaphoreSlim _writeLock = new SemaphoreSlim(1, 1);

        public override async Task QueryStreamAsync(
            QueryEnvelope queryEnvelope,
            IServerStreamWriter<ResponseEnvelope> responseStream,
            ServerCallContext callContext)
        {
            try
            {
                var broker = new GrpcBroker(queryEnvelope);
                await Run(broker, responseStream);
            }
            catch (Exception ex)
            {
                await ClientOnError(responseStream, ex);
                throw;
            }
        }

        private async Task Run(GrpcBroker broker, IServerStreamWriter<ResponseEnvelope> responseStream)
        {
            var isComplete = false;
            var isError = false;
            var eventCount = 0;

            using (var sub = broker.Observable.Subscribe(
                async e => await ClientOnNext(responseStream, e),
                async ex => await ClientOnError(responseStream, ex),
                async () => await ClientOnCompleted(responseStream)))
            {
                do
                {
                    var @event = Activator.CreateInstance(broker.SourceType);
                    broker.SourceType.GetProperty("CustomerId").SetValue(@event, Guid.NewGuid());
                    broker.SourceType.GetProperty("Email").SetValue(@event, "some-email@blah.com");
                    broker.SourceType.GetProperty("PhoneNumber").SetValue(@event, "09-" + random.Next(0, 6));

                    broker.Observer.OnNext(@event);

                    await Task.Delay(random.Next(0, 500));

                    if (eventCount++ > 5)
                    {
                        isComplete = random.Next(0, 20) == 0;
                        isError = !isComplete && random.Next(0, 20) == 0;
                    }
                } while (!isComplete && !isError);

                if (isComplete)
                {
                    broker.Observer.OnCompleted();
                }
                else if (isError)
                {
                    broker.Observer.OnError(new Exception("Example error"));
                }
            }
        }

        private async Task ClientOnNext(
            IServerStreamWriter<ResponseEnvelope> responseStream,
            object payload)
        {
            await SendEnvelopeToClient(responseStream, new ResponseEnvelope
            {
                PayloadType = payload.GetType().FullName,
                Payload = EnvelopeHelper.Pack(payload),
                RxMethod = ResponseEnvelope.Types.RxMethod.Next
            });
        }

        private async Task ClientOnCompleted(IServerStreamWriter<ResponseEnvelope> responseStream)
        {
            await SendEnvelopeToClient(responseStream, new ResponseEnvelope
            {
                PayloadType = "",
                Payload = "",
                RxMethod = ResponseEnvelope.Types.RxMethod.Completed
            });
        }

        private async Task ClientOnError(
            IServerStreamWriter<ResponseEnvelope> responseStream,
            Exception ex)
        {
            await SendEnvelopeToClient(responseStream, new ResponseEnvelope
            {
                PayloadType = ex.GetType().FullName,
                Payload = EnvelopeHelper.Pack(ex),
                RxMethod = ResponseEnvelope.Types.RxMethod.Error
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
