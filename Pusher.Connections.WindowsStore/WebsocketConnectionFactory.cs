using System;

namespace Pusher.Connections.WindowsStore
{
	public class WebsocketConnectionFactory : IConnectionFactory
	{
		public IConnection Create(Uri endpoint)
		{
			return new WebSocket(endpoint);
		}
	}
}
