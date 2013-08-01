using System.Runtime.Serialization;

namespace Pusher
{
    [DataContract]
    public class OutgoingEvent<T> : IOutgoingEvent<T>
    {
        [DataMember(Name = "event")]
        public string EventName { get; set; }

        [DataMember(Name = "channel", EmitDefaultValue = false, IsRequired = false)]
        public string Channel { get; set; }

        [DataMember(Name = "data")]
        public T Data { get; set; }
    }
}