using System;
using System.Runtime.Serialization;

namespace Pusher.Events
{
	[DataContract]
	public class SubscribeEvent : OutgoingEvent<SubscribeEventArgs>
	{
		public SubscribeEvent(string channel)
		{
			EventName = Pusher.EventSubscribe;
            Data = new SubscribeEventArgs { Channel = channel };
		}

        [DataMember(Name = "event")]
        public string EventName { get; set; }

	    public string Channel { get; set; }
	}
}