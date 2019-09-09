using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Newtonsoft.Json;
using Qube.Core;
using Qube.Core.Types;
using Qube.Core.Utils;

namespace Qube.Grpc
{
    public class GrpcBroker
    {
        private Subject<object> _subject;

        public Type SourceType { get; private set; }
        public Type[] RegisteredTypes { get; private set; }

        public IObserver<object> Observer { get { return _subject; } }
        public IObservable<object> Observable { get; private set; }

        public GrpcBroker(QueryEnvelope queryEnvelope)
        {
            var registeredTypeDefinitions = JsonConvert.DeserializeObject<PortableTypeDefinition[]>(queryEnvelope.RegisteredTypes);
            RegisteredTypes = new PortableTypeBuilder().BuildTypes(registeredTypeDefinitions);
            SourceType = RegisteredTypes.Single(t => t.FullName == queryEnvelope.SourceTypeName);

            _subject = new Subject<object>();

            var queryExpression = SerializationHelper.DeserializeLinqExpression(queryEnvelope.Payload, RegisteredTypes);
            
            Observable = new ServerQueryObservable<object>(SourceType, _subject.AsQbservable(), queryExpression);
        }
    }
}
