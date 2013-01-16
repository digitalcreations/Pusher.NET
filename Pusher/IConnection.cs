using System;
using System.Threading.Tasks;
using Pusher.Events;

namespace Pusher
{
	public interface IConnection
	{
		void Close();
		Task Open();
		Task SendMessage(string data);
		event EventHandler<EventArgs> OnClose;
		event EventHandler<EventArgs> OnOpen;
		event EventHandler<DataReceivedEventArgs> OnData;
	}
}