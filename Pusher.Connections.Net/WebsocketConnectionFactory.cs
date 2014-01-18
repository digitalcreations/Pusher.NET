using System;

namespace Pusher.Connections.Net
{
    public class WebSocketConnectionFactory : IConnectionFactory
    {
        public IConnection Create(Uri endpoint)
        {
            return new WebSocketConnection(endpoint);
        }
    }
}
