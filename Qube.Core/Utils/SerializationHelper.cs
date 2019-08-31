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
            expressionSerializer.AddKnownType(typeof(StringSplitOptions));  // TODO: Have this come in from above
            return expressionSerializer;
        }

        public static string SerializeLinqExpression<TIn, TOut>(Expression expression)
        {
            ExpressionSerializer expressionSerializer = NewExpressionSerializer
            (
                typeof(StringSplitOptions),
                typeof(CustomerCreatedEvent),
                typeof(BaseEvent),
                typeof(IDomainEvent)
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
