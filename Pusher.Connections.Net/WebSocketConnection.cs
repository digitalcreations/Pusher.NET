using System;
using System.Threading.Tasks;
using WebSocket4Net;
using DataReceivedEventArgs = Pusher.Events.DataReceivedEventArgs;

namespace Pusher.Connections.Net
{
    public class WebSocketConnection : IConnection
    {
        private WebSocket _socket;
        private readonly Uri _endpoint;
        private ConnectionState _connectionState;

        public WebSocketConnection(Uri endpoint)
        {
            _endpoint = endpoint;
            SetupSocket();
        }

        private void SetupSocket()
        {
            _socket = new WebSocket(_endpoint.ToString());
            _socket.Closed += OnSocketClosed;
            _socket.MessageReceived += OnMessageReceived;
        }

        private async void OnMessageReceived(object sender, MessageReceivedEventArgs args)
        {
            if (OnData == null) return;

            var exceptionOccured = false;
            try
            {
                var text = args.Message;
                OnData(sender, new DataReceivedEventArgs { TextData = text });
            }
            catch (Exception)
            {
                exceptionOccured = true;
            }
            // cannot await in catch
            if (exceptionOccured) await Reconnect();
        }

        private async Task Reconnect()
        {
            if (_connectionState == ConnectionState.Connecting || _connectionState == ConnectionState.Connected) return;

            SetupSocket();
            await Open();
        }

        private async void OnSocketClosed(object sender, EventArgs args)
        {
            if (_connectionState != ConnectionState.Disconnecting)
            {
                await Reconnect();
            }
            _connectionState = ConnectionState.Disconnected;
            if (OnClose != null) OnClose(sender, new EventArgs());
        }

        #region Implementation of IConnection

        public void Close()
        {
            _connectionState = ConnectionState.Disconnecting;
            _socket.Close(1000, "Close requested");
            _connectionState = ConnectionState.Disconnected;
        }

        public async Task Open()
        {
            if (_connectionState == ConnectionState.Connected)
            {
                Close();
                SetupSocket();
            }

            _connectionState = ConnectionState.Connecting;
            _socket.Open();
            
            _connectionState = ConnectionState.Connected;
            if (OnOpen != null)
            {
                OnOpen(this, new EventArgs());
            }
        }

        public async Task SendMessage(string data)
        {
            if (_connectionState != ConnectionState.Connected)
            {
                await Open();
            }

            _socket.Send(data);
        }

        public event EventHandler<EventArgs> OnClose;
        public event EventHandler<EventArgs> OnOpen;
        public event EventHandler<DataReceivedEventArgs> OnData;

        #endregion
    }
}
