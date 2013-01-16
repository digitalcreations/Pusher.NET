using System;

namespace Pusher.Events
{
	public class DataReceivedEventArgs : EventArgs
	{
		public string TextData { get; set; }
	}
}