using Serialize.Linq.Serializers;
using System;
using System.Collections.Generic;
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
            expressionSerializer.AddKnownType(typeof(Dictionary<string, object>));
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

        public static LambdaExpression DeserializeLinqExpression(string expressionString, params Type[] knownTypes)
        {
            var expressionSerializer = NewExpressionSerializer(knownTypes);

            var expression = (LambdaExpression)expressionSerializer.DeserializeText(expressionString);
            return expression;
        }
    }
}
