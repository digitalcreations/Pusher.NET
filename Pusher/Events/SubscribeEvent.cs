using System;
using System.Runtime.Serialization;

namespace Pusher.Events
{
	[DataContract]
	public class SubscribeEvent : IEvent
	{
		public SubscribeEvent(string channel)
		{
			EventName = Pusher.EventSubscribe;
            DataObject = new SubscribeEventArgs { Channel = channel };
		}

        [DataMember(Name = "event")]
        public string EventName { get; set; }

	    public string Channel { get; set; }

	    public Type GetDataType()
	    {
	        return typeof (SubscribeEventArgs);
	    }

	    [DataMember(Name = "data")]
        public SubscribeEventArgs DataObject { get; set; }
	}
}