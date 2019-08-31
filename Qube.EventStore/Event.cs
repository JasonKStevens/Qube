using System;

namespace Qube.EventStore
{
    /// <summary>
    /// Representation of the event record that the client qbservable sees
    /// and queries over.
    /// </summary>
    public class Event
    {
        /// <summary>
        /// The Event Stream that this event belongs to
        /// </summary>
        public string EventStreamId;

        /// <summary>
        /// The Unique Identifier representing this event
        /// </summary>
        public Guid EventId;

        /// <summary>
        /// The number of this event in the stream
        /// </summary>
        public long EventNumber;

        /// <summary>
        /// The type of event this is
        /// </summary>
        public string EventType;

        /// <summary>
        /// The serialized data of this event
        /// </summary>
        public string Data;

        /// <summary>
        /// A serialized metadata associated with this event
        /// </summary>
        public string Metadata;

        /// <summary>
        /// Indicates whether the content is internally marked as json
        /// </summary>
        public bool IsJson;

        /// <summary>
        /// A datetime representing when this event was created in the system
        /// </summary>
        public DateTime Created;
    }
}
