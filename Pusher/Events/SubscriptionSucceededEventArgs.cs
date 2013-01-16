using System.Runtime.Serialization;

namespace Pusher.Events
{
	[DataContract]
	public class SubscriptionSucceededEventArgs
	{
		[DataMember(Name = "presence")]
		public PresenceData Presence { get; set; }
	}
}