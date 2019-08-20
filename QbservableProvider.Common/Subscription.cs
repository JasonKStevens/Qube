using Grpc.Core;
using Grpc.Net.Client;
using QbservableProvider.Common.Protos;
using System;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace QbservableProvider.Common
{
    internal class Subscription : IDisposable
    {
        private static readonly HttpClient _httpClient;

        private readonly string _url;
        private readonly Expression _expression;
        private readonly CancellationTokenSource _cancelSource;

        private AsyncServerStreamingCall<EventEnvelope> _streamingCall;

        internal Subscription(string url, Expression expression)
        {
            _url = url;
            _expression = expression;
            _cancelSource = new CancellationTokenSource();
        }

        internal void Subscribe<T>(IObserver<T> observer)
        {
            var seralizedExpressoin = SerializationHelper.SerializeLinqExpression<T>(_expression);

            var httpClient = _httpClient ?? new HttpClient { BaseAddress = new Uri(_url) };  // TODO: Use HttpClientFactory
            var client = GrpcClient.Create<StreamService.StreamServiceClient>(httpClient);

            var queryEnvelope = new QueryEnvelope { Payload = seralizedExpressoin };
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
                var payload = SerializationHelper.Unpack<T>(@event);

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
