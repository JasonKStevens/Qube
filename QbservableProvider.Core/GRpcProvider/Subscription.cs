using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace QbservableProvider.Core.GRpcProvider
{
    internal class Subscription : IDisposable
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly StreamDbContextOptions _options;
        private readonly CancellationTokenSource _cancelSource;

        private AsyncServerStreamingCall<EventEnvelope> _streamingCall;

        internal Subscription(IHttpClientFactory httpClientFactory, StreamDbContextOptions options)
        {
            _httpClientFactory = httpClientFactory;
            _options = options;
            _cancelSource = new CancellationTokenSource();
        }

        internal void Connect<TIn, TOut>(Expression expression, IObserver<TOut> observer)
        {
            var seralizedExpression = SerializationHelper.SerializeLinqExpression<TIn, TOut>(expression);

            var httpClient = _httpClientFactory.CreateClient("GrpcProvider");
            httpClient.BaseAddress = new Uri(_options.ConnectionString);

            var client = GrpcClient.Create<StreamService.StreamServiceClient>(httpClient);

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
