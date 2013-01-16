using System;

namespace Pusher.Exceptions
{
	public class MessageForNonSubscribedChannelException : ArgumentOutOfRangeException
	{
		public MessageForNonSubscribedChannelException(string name)
			: base(string.Format("A message was received for '{0}', but no such channel can be found", name))
		{
		}
	}
}