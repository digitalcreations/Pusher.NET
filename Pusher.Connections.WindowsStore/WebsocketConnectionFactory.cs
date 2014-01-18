using System;

namespace Pusher.Connections.WindowsStore
{
	public class WebSocketConnectionFactory : IConnectionFactory
	{
		public IConnection Create(Uri endpoint)
		{
			return new WebSocketConnection(endpoint);
		}
	}
}
