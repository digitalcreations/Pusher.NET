using System;

namespace Pusher
{
	public interface IEventContract
	{
		string Name { get; }
		Type DeserializeAs { get; }
	}
}