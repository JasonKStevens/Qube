using Newtonsoft.Json;

namespace Qube.EventStore.Utils
{
    public static class EnvelopeHelper
    {
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
