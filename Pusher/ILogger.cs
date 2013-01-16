namespace Pusher
{
	public interface ILogger
	{
		void Debug(string text, params object[] parameters);
	}
}