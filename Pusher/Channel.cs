namespace Pusher
{
	public class Channel : EventEmitter
	{
		internal Channel(string name)
		{
			Name = name;
		}

		public string Name { get; private set; }

		public bool Established { get; internal set; }
	}
}