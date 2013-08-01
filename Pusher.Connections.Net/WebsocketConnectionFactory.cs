using System;

namespace Pusher.Connections.Net
{
    public class WebsocketConnectionFactory : IConnectionFactory
    {
        public IConnection Create(Uri endpoint)
        {
            return new WebSocketConnection(endpoint);
        }
    }
}
