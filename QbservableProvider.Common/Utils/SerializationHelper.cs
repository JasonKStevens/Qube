using Newtonsoft.Json;
using QbservableProvider.Common.Protos;
using Serialize.Linq.Serializers;
using System;
using System.Linq.Expressions;
using System.Reactive.Linq;
using JsonSerializer = Serialize.Linq.Serializers.JsonSerializer;

namespace QbservableProvider.Common
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

        internal static ParameterExpression NewObserverParameter()
        {
            return Expression.Parameter(typeof(IQbservable<Event>), "o");
        }

        public static Expression Deserialise(string expressionString)
        {
            var serializer = new ExpressionSerializer(new JsonSerializer());
            var expression = serializer.DeserializeText(expressionString);
            return expression;
        }

        public static EventEnvelope Pack(object payload, Type returnType)
        {
            return new EventEnvelope
            {
                Payload = JsonConvert.SerializeObject(payload)
            };
        }

        internal static string SerializeLinqExpression<T>(Expression expression)
        {
            ExpressionSerializer expressionSerializer = NewExpressionSerializer
            (
                typeof(StringSplitOptions)
            );

            var parameter = SerializationHelper.NewObserverParameter();
            var lambda = Expression.Lambda<Func<IQbservable<Event>, IQbservable<T>>>(expression, parameter);
            var serializedLambda = expressionSerializer.SerializeText(lambda);
            return serializedLambda;
        }

        public static T Unpack<T>(EventEnvelope @event)
        {
            return typeof(T).Name.Contains("AnonymousType") ?
                JsonConvert.DeserializeAnonymousType(@event.Payload, default(T)) :
                JsonConvert.DeserializeObject<T>(@event.Payload);
        }
    }
}
