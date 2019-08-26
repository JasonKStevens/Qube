using Newtonsoft.Json;
using Serialize.Linq.Serializers;
using System;
using System.Linq.Expressions;
using System.Reactive.Linq;
using JsonSerializer = Serialize.Linq.Serializers.JsonSerializer;

namespace QbservableProvider.Core
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

        // TODO: Move away from here
        public static ParameterExpression NewObserverParameter<T>()
        {
            return Expression.Parameter(typeof(IQbservable<T>), "o");
        }

        public static string SerializeLinqExpression<TIn, TOut>(Expression expression)
        {
            ExpressionSerializer expressionSerializer = NewExpressionSerializer
            (
                typeof(StringSplitOptions)
            );

            // TODO: Pull this expression stuff out
            var parameter = NewObserverParameter<TIn>();
            var lambda = Expression.Lambda<Func<IQbservable<TIn>, IQbservable<TOut>>>(expression, parameter);
            var serializedLambda = expressionSerializer.SerializeText(lambda);
            return serializedLambda;
        }

        public static Expression DeserializeLinqExpression(string expressionString)
        {
            var serializer = new ExpressionSerializer(new JsonSerializer());
            var expression = serializer.DeserializeText(expressionString);
            return expression;
        }

        public static EventEnvelope Pack(object payload)
        {
            return new EventEnvelope
            {
                Payload = JsonConvert.SerializeObject(payload)
            };
        }

        public static T Unpack<T>(string payload)
        {
            return typeof(T).Name.Contains("AnonymousType") ?
                JsonConvert.DeserializeAnonymousType(payload, default(T)) :
                JsonConvert.DeserializeObject<T>(payload);
        }
    }
}
