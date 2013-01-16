using System.Runtime.Serialization;

namespace Pusher.Events
{
	[DataContract]
	public class SubscribeEventArgs
	{
		[DataMember(Name = "channel")]
		public string Channel { get; set; }

		[DataMember(Name = "auth", EmitDefaultValue = false, IsRequired = false)]
		public string Auth { get; set; }

		[DataMember(Name = "channel_data", EmitDefaultValue = false, IsRequired = false)]
		public object ChannelData { get; set; }
	}
}