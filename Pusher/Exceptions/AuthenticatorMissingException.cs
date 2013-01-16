using System;

namespace Pusher.Exceptions
{
	public class AuthenticatorMissingException : MissingMemberException
	{
		public AuthenticatorMissingException(string channel)
			: base(string.Format("Missing authenticator for subscribing to '{0}'.", channel))
		{
		}
	}
}