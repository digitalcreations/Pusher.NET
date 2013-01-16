using System;

namespace Pusher.Exceptions
{
	public class EventNameInUseException : ArgumentException
	{
		public EventNameInUseException(string name)
			: base(string.Format("The event '{0}' is already served by a different contract.", name))
		{
		}
	}
}