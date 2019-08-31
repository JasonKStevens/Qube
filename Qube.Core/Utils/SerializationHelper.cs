using Serialize.Linq.Serializers;
using System;
using System.Linq.Expressions;
using System.Reactive.Linq;
using JsonSerializer = Serialize.Linq.Serializers.JsonSerializer;

namespace Qube.Core
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

        public static string SerializeLinqExpression<TIn, TOut>(Expression expression)
        {
            // TODO: Not sure how useful but if so have it come in from above
            var expressionSerializer = NewExpressionSerializer
            (
                typeof(StringSplitOptions)
            );

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
