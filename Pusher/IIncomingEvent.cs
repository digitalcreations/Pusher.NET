using System.Runtime.Serialization;

namespace Pusher
{
    public interface IIncomingEvent
    {
        [DataMember(Name = "event")]
        string EventName { get; set; }

        [DataMember(Name = "channel", EmitDefaultValue = false, IsRequired = false)]
        string Channel { get; set; }

        [DataMember(Name = "data")]
        string Data { get; set; }
    }

    public interface IIncomingEvent<T> : IIncomingEvent
    {
        T DataObject { get; set; }
    }
}
