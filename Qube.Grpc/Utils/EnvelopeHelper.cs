using Newtonsoft.Json;
using System;

namespace Qube.Grpc.Utils
{
    public static class EnvelopeHelper
    {
        public static string Pack(object payload)
        {
            return JsonConvert.SerializeObject(payload);
        }

        public static T Unpack<T>(string payload, Type type = null)
        {
            return typeof(T).Name.Contains("AnonymousType") ?
                JsonConvert.DeserializeAnonymousType(payload, default(T)) :
                type != null ? (T) JsonConvert.DeserializeObject(payload, type) : JsonConvert.DeserializeObject<T>(payload);
        }
    }
}
