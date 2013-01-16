using System;

namespace Pusher
{
	public interface IConnectionFactory
	{
		IConnection Create(Uri endpoint);
	}
}