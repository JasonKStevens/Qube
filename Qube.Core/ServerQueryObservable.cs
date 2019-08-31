using Qube.Core;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;

namespace QbservableProvider.Core
{
    /// <summary>
    /// An observable with the query expression compiled over the given source qbservable.
    /// </summary>
    /// <typeparam name="TIn">The base event type of the subject.</typeparam>
    public class ServerQueryObservable<TIn, TOut> : IObservable<TOut>
    {
        private readonly IQbservable<TOut> _qbservable;

        public ServerQueryObservable(
            IQbservable<TIn> sourceQbservable,
            LambdaExpression queryExpression)
        {
            _qbservable = (IQbservable<TOut>) CastStreamToOutputType(queryExpression)
                .Compile()
                .DynamicInvoke(sourceQbservable);
        }

        public IDisposable Subscribe(IObserver<TOut> observer)
        {
            return _qbservable.Subscribe(observer);
        }

        /// <summary>
        /// Call .Cast<TOut>() on Rx query's lambda expression.
        /// </summary>
        private static LambdaExpression CastStreamToOutputType(LambdaExpression lambdaExpression)
        {
            var castMethod = typeof(Qbservable)
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where(mi => mi.Name == "Cast")
                .Single()
                .MakeGenericMethod(typeof(TOut));

            var methodCall = Expression.Call(null, castMethod, lambdaExpression.Body);
            var castLambdaExpression = Expression.Lambda(methodCall, lambdaExpression.Parameters);

            return castLambdaExpression;
        }
    }
}
