using Grpc.Core;
using Qube.Core;
using Qube.EventStore.Utils;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Qube.EventStore
{
    internal class Subscription : IDisposable
    {
        private readonly StreamDbContextOptions _options;
        private readonly CancellationTokenSource _cancelSource;

        private AsyncServerStreamingCall<EventEnvelope> _streamingCall;

        internal Subscription(StreamDbContextOptions options)
        {
            _options = options;
            _cancelSource = new CancellationTokenSource();
        }

        internal void Connect<TIn, TOut>(Expression expression, IObserver<TOut> observer)
        {
            var seralizedExpression = SerializationHelper.SerializeLinqExpression<TIn, TOut>(expression);

            Channel channel = new Channel(_options.ConnectionString, ChannelCredentials.Insecure);
            var client = new StreamService.StreamServiceClient(channel.CreateCallInvoker());

            var queryEnvelope = new QueryEnvelope
            {
                Payload = seralizedExpression,
                TypeIn = typeof(TIn).FullName,
                TypeOut = typeof(TOut).FullName
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
                    await ListenToResponseStream<T>(observer.OnNext, _cancelSource.Token);
                }
                catch (Exception ex)
                {
                    observer.OnError(ex);
                    return;
                }

                observer.OnCompleted();
            };
        }

        private async Task ListenToResponseStream<T>(Action<T> onNext, CancellationToken token)
        {
            while (!token.IsCancellationRequested &&
                    await _streamingCall.ResponseStream.MoveNext(token))
            {
                var @event = _streamingCall.ResponseStream.Current;

                if (!string.IsNullOrEmpty(@event.Error))
                {
                    // TODO: More efficient not to throw here
                    throw new Exception(@event.Error);
                }

                var payload = EnvelopeHelper.Unpack<T>(@event.Payload);
                onNext(payload);
            }

            if (token.IsCancellationRequested)
            {
                throw new TaskCanceledException("Remote subscription was cancelled");
            }
        }

        public void Dispose()
        {
            _cancelSource.Cancel();
            _streamingCall?.Dispose();
        }
    }
}
