using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Grpc.Core;
using Qube.Core;
using Qube.Core.Types;
using Qube.Grpc.Utils;
using RxMethod = Qube.Grpc.ResponseEnvelope.Types.RxMethod;
using Qube.Core.Utils;

namespace Qube.Grpc
{
    public class Subscription : IDisposable
    {
        private readonly StreamDbContextOptions _options;
        private readonly string[] _streamPatterns;
        private readonly CancellationTokenSource _cancelSource;

        private AsyncServerStreamingCall<ResponseEnvelope> _streamingCall;
        private PortableTypeDefinition[] _types;

        public Subscription(StreamDbContextOptions options, string[] streamPatterns)
        {
            _options = options;
            _streamPatterns = streamPatterns;
            _cancelSource = new CancellationTokenSource();
        }

        public void Connect<TIn, TOut>(Expression expression, IObserver<TOut> observer)
        {
            var seralizedExpression = SerializationHelper.SerializeLinqExpression<TIn, TOut>(
                expression,
                _options.TypesToTransfer.ToArray());

            Channel channel = new Channel(_options.ConnectionString, ChannelCredentials.Insecure);
            var client = new StreamService.StreamServiceClient(channel.CreateCallInvoker());

            var definer = new PortableTypeDefiner();
            var classDefinition = definer.BuildDefinition(typeof(TIn));
            var enums = definer.BuildDefinitions(_options.TypesToTransfer.Where(t => t.IsEnum).ToArray());
            var types = definer.BuildDefinitions(_options.TypesToTransfer.Where(t => !t.IsEnum).ToArray());

            _types = enums
                .Concat(new[] { classDefinition })
                .Concat(types)
                .GroupBy(t => new { t.AssemblyName, t.ClassName })
                .Select(g => g.First())
                .ToArray();

            var queryEnvelope = new QueryEnvelope
            {
                Payload = seralizedExpression,
                SourceTypeName = classDefinition.ClassName,
                RegisteredTypes = JsonConvert.SerializeObject(_types),
                StreamPattern = JsonConvert.SerializeObject(_streamPatterns)
            };

            _streamingCall = client.QueryStreamAsync(queryEnvelope);

            // TODO: No need for one thread per subscription - this can be made more efficient
            Task.Run(GetObserveTask(observer), _cancelSource.Token)
                .ContinueWith(_ => _cancelSource.Dispose());
        }

        private Func<Task> GetObserveTask<T>(IObserver<T> observer)
        {
            return async () =>
            {
                try
                {
                    await ListenToResponseStream<T>(
                        observer.OnNext,
                        observer.OnCompleted,
                        observer.OnError,
                        _cancelSource.Token);
                }
                catch (Exception ex)
                {
                    observer.OnError(ex);
                }
            };
        }

        private async Task ListenToResponseStream<T>(
            Action<T> onNext,
            Action onCompleted,
            Action<Exception> onError,
            CancellationToken token)
        {
            while (!token.IsCancellationRequested &&
                    await _streamingCall.ResponseStream.MoveNext(token))
            {
                var @event = _streamingCall.ResponseStream.Current;

                switch (@event.RxMethod)
                {
                    case RxMethod.Next:
                        var payloadType = _options.TypesToTransfer.Single(t => @event.PayloadType.IndexOf(t.FullName) != -1);
                        var payload = EnvelopeHelper.Unpack<T>(@event.Payload, payloadType);
                        onNext(payload);
                        break;

                    case RxMethod.Completed:
                        onCompleted();
                        return;

                    case RxMethod.Error:
                        var ex = EnvelopeHelper.Unpack<Exception>(@event.Payload);
                        onError(ex);
                        return;
                }
            }

            var message = token.IsCancellationRequested ? "Subscription was cancelled" : "Stream ended abruptly";
            onError(new TaskCanceledException(message));
        }

        public void Dispose()
        {
            _cancelSource.Cancel();
            _streamingCall?.Dispose();
        }
    }
}
