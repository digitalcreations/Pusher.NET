using System;
using System.Runtime.Serialization;

namespace Pusher
{
    public interface IEvent
    {
        [DataMember(Name = "event")]
        string EventName { get; set; }

        [DataMember(Name = "channel", EmitDefaultValue = false, IsRequired = false)]
        string Channel { get; set; }

        Type GetDataType();
    }
}
