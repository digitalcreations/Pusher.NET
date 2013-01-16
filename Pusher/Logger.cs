using System;

namespace Pusher
{
	internal class Logger : ILogger
	{
		private string WrapInfo(string text, string mode = "")
		{
			return string.Format("[{0}] {1:T} - {2}", mode, DateTime.Now, text);
		}

		#region Implementation of ILogger

		public void Debug(string text, params object[] parameters)
		{
#if DEBUG
			System.Diagnostics.Debug.WriteLine(WrapInfo(string.Format(text, parameters), "Debug"));
#endif
		}

		#endregion
	}
}