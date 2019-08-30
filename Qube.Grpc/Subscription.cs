﻿using Grpc.Core;
using Qube.Core;
using Qube.Grpc.Utils;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ResponseType = Qube.Grpc.ResponseEnvelope.Types.ResponseType;

namespace Qube.Grpc
{
    internal class Subscription : IDisposable
    {
        private readonly StreamDbContextOptions _options;
        private readonly CancellationTokenSource _cancelSource;

        private AsyncServerStreamingCall<ResponseEnvelope> _streamingCall;

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

            var queryEnvelope = new QueryEnvelope { Payload = seralizedExpression };
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

                switch (@event.ResponseType)
                {
                    case ResponseType.Next:
                        var payload = EnvelopeHelper.Unpack<T>(@event.Payload);
                        onNext(payload);
                        break;

                    case ResponseType.Completed:
                        onCompleted();
                        return;

                    case ResponseType.Error:
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
