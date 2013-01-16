using System.Runtime.Serialization;

namespace Pusher
{
	[DataContract]
	public class AuthenticationData : IAuthenticationData
	{
		#region Implementation of IAuthenticationData

		[DataMember(Name = "auth")]
		public string Auth { get; set; }

		[DataMember(Name = "channel_data")]
		public object ChannelData { get; set; }

		#endregion
	}
}