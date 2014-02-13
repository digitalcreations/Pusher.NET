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
        /// <summary>
        /// Any exceptions thrown outside of calls to any of the methods 
        /// above, should not be thrown, but raise OnError.
        /// 
        /// This means that it is OK for Close(), Open() and SendMessage()
        /// to throw exceptions. However, any internal reconnect logic 
        /// that fails should report it here. Also, if the connection is broken
        /// this should be raised.
        /// </summary>
	    event EventHandler<ExceptionEventArgs> OnError;
		event EventHandler<EventArgs> OnClose;
		event EventHandler<EventArgs> OnOpen;
		event EventHandler<DataReceivedEventArgs> OnData;
	}
}