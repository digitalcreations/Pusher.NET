using System.Runtime.Serialization;

namespace Pusher.Events
{
	[DataContract]
	public class SubscribeEvent : Event<SubscribeEventArgs>
	{
		public SubscribeEvent(string channel)
		{
			EventName = Pusher.EventSubscribe;
			DataObject = new SubscribeEventArgs { Channel = channel };
		}
	}
}