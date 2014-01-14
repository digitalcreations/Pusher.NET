using System.Threading.Tasks;

namespace Pusher
{
	public interface IAuthenticator
	{
		Task<IAuthenticationData> AuthenticateAsync(string socketId, string channelName);
	}
}