namespace Pusher
{
	public interface IAuthenticationData
	{
		string Auth { get; }
		object ChannelData { get; }
	}
}