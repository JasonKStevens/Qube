using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;

namespace Qube.Core
{
    /// <summary>
    /// An observable with the query expression compiled over the given source qbservable.
    /// </summary>
    public class ServerQueryObservable<TOut> : IObservable<TOut>
    {
        private readonly IQbservable<TOut> _qbservable;

        public ServerQueryObservable(
            Type sourceType,
            IQbservable<object> sourceQbservable,
            LambdaExpression queryExpression)
        {
            var castSourceQbservable = GetCastMethod(sourceType)
                .Invoke(null, new[] { sourceQbservable });

            _qbservable = (IQbservable<TOut>)CastStreamToOutputType(queryExpression)
                .Compile()
                .DynamicInvoke(castSourceQbservable);
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
            var castMethod = GetCastMethod(typeof(TOut));

            var methodCall = Expression.Call(null, castMethod, lambdaExpression.Body);
            var castLambdaExpression = Expression.Lambda(methodCall, lambdaExpression.Parameters);

            return castLambdaExpression;
        }

        private static MethodInfo GetCastMethod(Type type)
        {
            return typeof(Qbservable)
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where(mi => mi.Name == "Cast")
                .Single()
                .MakeGenericMethod(type);
        }
    }
}
