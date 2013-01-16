using System.Runtime.Serialization;

namespace Pusher.Events
{
	[DataContract]
	public class ConnectionEstablishedEventArgs
	{
		[DataMember(Name = "socket_id")]
		public string SocketId { get; set; }
	}
}