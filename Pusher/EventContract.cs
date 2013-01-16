using System;

namespace Pusher
{
	public class EventContract : IEventContract
	{
		protected EventContract(string name, Type deserializeAs)
		{
			Name = name;
			DeserializeAs = deserializeAs;
		}

		public string Name { get; private set; }
		public Type DeserializeAs { get; private set; }

		public static EventContract Create<T>(string name)
		{
			return new EventContract(name, typeof (T));
		}

		public static EventContract Create(string name, Type type)
		{
			return new EventContract(name, type);
		}

		public static EventContract Create(string name)
		{
			return new EventContract(name, typeof (String));
		}
	}
}