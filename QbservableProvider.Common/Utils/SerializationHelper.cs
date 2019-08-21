using Newtonsoft.Json;
using QbservableProvider.Core.GenericGRpcProvider;
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

        internal static ExpressionSerializer NewExpressionSerializer(params Type[] knownTypes)
        {
            var expressionSerializer = new ExpressionSerializer(JsonSerializer);
            expressionSerializer.AddKnownType(typeof(StringSplitOptions));  // TODO: Have this come in from above
            return expressionSerializer;
        }

        internal static ParameterExpression NewObserverParameter<T>()
        {
            return Expression.Parameter(typeof(IQbservable<T>), "o");
        }

        internal static string SerializeLinqExpression<TIn, TOut>(Expression expression)
        {
            ExpressionSerializer expressionSerializer = NewExpressionSerializer
            (
                typeof(StringSplitOptions)
            );

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

        public static T Unpack<T>(EventEnvelope @event)
        {
            return typeof(T).Name.Contains("AnonymousType") ?
                JsonConvert.DeserializeAnonymousType(@event.Payload, default(T)) :
                JsonConvert.DeserializeObject<T>(@event.Payload);
        }
    }
}
