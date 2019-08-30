using Newtonsoft.Json;

namespace Qube.Grpc.Utils
{
    public static class EnvelopeHelper
    {
        public static string Pack(object payload)
        {
            return JsonConvert.SerializeObject(payload);
        }

        public static T Unpack<T>(string payload)
        {
            return typeof(T).Name.Contains("AnonymousType") ?
                JsonConvert.DeserializeAnonymousType(payload, default(T)) :
                JsonConvert.DeserializeObject<T>(payload);
        }
    }
}
