using Serialize.Linq.Serializers;
using System;
using System.Linq.Expressions;
using System.Reactive.Linq;
using JsonSerializer = Serialize.Linq.Serializers.JsonSerializer;

namespace Qube.Core.Utils
{
    public static class SerializationHelper
    {
        private static readonly JsonSerializer JsonSerializer = new JsonSerializer();

        public static ExpressionSerializer NewExpressionSerializer(params Type[] knownTypes)
        {
            var expressionSerializer = new ExpressionSerializer(JsonSerializer);
            expressionSerializer.AddKnownTypes(knownTypes);
            return expressionSerializer;
        }

        public static string SerializeLinqExpression<TIn, TOut>(Expression expression, params Type[] knownTypes)
        {
            var expressionSerializer = NewExpressionSerializer(knownTypes);

            // TODO: Pull this expression stuff out
            var parameter = Expression.Parameter(typeof(IQbservable<TIn>), "o");
            var lambda = Expression.Lambda<Func<IQbservable<TIn>, IQbservable<TOut>>>(expression, parameter);
            var serializedLambda = expressionSerializer.SerializeText(lambda);
            return serializedLambda;
        }

        public static LambdaExpression DeserializeLinqExpression(string expressionString)
        {
            var serializer = new ExpressionSerializer(new JsonSerializer());
            var expression = (LambdaExpression) serializer.DeserializeText(expressionString);
            return expression;
        }
    }
}
