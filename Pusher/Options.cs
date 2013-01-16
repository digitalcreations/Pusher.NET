namespace Pusher
{
	public class Options
	{
		public Options()
		{
			Scheme = WebServiceScheme.PlainText;
		}

		public WebServiceScheme Scheme { get; set; }
		public IAuthenticator Authenticator { get; set; }
		public bool RaiseAllEventsOnPusher { get; set; }

		internal int Port
		{
			get { return Scheme == WebServiceScheme.PlainText ? 80 : 443; }
		}

		internal string SchemeString
		{
			get { return Scheme == WebServiceScheme.PlainText ? "ws" : "wss"; }
		}
	}
}