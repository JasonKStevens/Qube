using System;
using System.Linq.Expressions;
using System.Reactive.Linq;

namespace QbservableProvider.Core.InMemoryProvider
{
    internal class Subscription<T> : IDisposable
    {
        private readonly MethodCallExpression _expression;
        private readonly IObservable<T> _observable;
        private readonly StreamDbContextOptions _options;
        private IDisposable _sub;

        internal Subscription(Expression expression, IObservable<T> observable, StreamDbContextOptions options)
        {
            _expression = (expression as MethodCallExpression) ?? throw new ArgumentException("Expression is expected to be a method call");
            _observable = observable;
            _options = options;
        }

        internal void Attach(IObserver<T> observer)
        {
            _sub = BuildQbservable()
                .Subscribe(observer);
        }

        private IQbservable<T> BuildQbservable()
        {
            // TODO: Change IQbservable<object> to IQbservable<TIn> by reflecting over _expression

            var parameter = SerializationHelper.NewObserverParameter<T>();
            var lambdaExpression = Expression.Lambda<Func<IQbservable<object>, IQbservable<T>>>(_expression, parameter);
            var lambda = lambdaExpression.Compile();

            var qbservable = ((IQbservable<T>) lambda.DynamicInvoke(_observable));
            return qbservable;
        }

        public void Dispose()
        {
            _sub?.Dispose();
        }
    }
}
