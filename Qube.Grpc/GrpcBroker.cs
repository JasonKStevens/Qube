using Newtonsoft.Json;
using Qube.Core;
using Qube.Core.Utils;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Qube.Grpc
{
    public class GrpcBroker : IDisposable
    {
        public Type SourceType { get; private set; }
        public Type[] RegisteredTypes { get; private set; }

        public Subject<object> Subject { get; private set; }
        public ServerQueryObservable<object> Observable { get; private set; }

        public GrpcBroker(QueryEnvelope queryEnvelope)
        {
            var registeredTypeDefinitions = JsonConvert.DeserializeObject<PortableTypeDefinition[]>(queryEnvelope.RegisteredTypes);
            RegisteredTypes = new PortableTypeBuilder().BuildTypes(registeredTypeDefinitions);

            SourceType = RegisteredTypes.Where(t => t.FullName == queryEnvelope.SourceTypeName).Single();

            var queryExpression = SerializationHelper.DeserializeLinqExpression(queryEnvelope.Payload);
            Subject = new Subject<object>();

            Observable = new ServerQueryObservable<object>(SourceType, Subject.AsQbservable(), queryExpression);
        }

        public void Dispose()
        {
            Subject.Dispose();
        }
    }
}
