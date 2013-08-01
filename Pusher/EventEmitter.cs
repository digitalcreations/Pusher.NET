using System;
using System.Collections.Generic;

namespace Pusher
{
	public abstract class EventEmitter
	{
		public delegate void EventEmittedHandler(object sender, IIncomingEvent e);

		public delegate void GenericEventEmittedHandler<T>(object sender, IIncomingEvent<T> e);

		private readonly IDictionary<Type, object> _subscriptions = new Dictionary<Type, object>();

		protected internal void EmitEvent(IIncomingEvent e)
		{
			if (EventEmitted != null)
			{
				EventEmitted(this, e);
			}
		}

		public event EventEmittedHandler EventEmitted;

		public EventSubscription<T> GetEventSubscription<T>()
		{
			var type = typeof (T);
			if (!_subscriptions.ContainsKey(type))
			{
				var subscription = new EventSubscription<T>();
				_subscriptions[type] = subscription;
				EventEmitted += subscription.TryConvertEvent;
			}

			return _subscriptions[type] as EventSubscription<T>;
		}

		public class EventSubscription<T>
		{
			public event GenericEventEmittedHandler<T> EventEmitted;

			public void TryConvertEvent(object sender, IIncomingEvent e)
			{
				if (e is IIncomingEvent<T> && EventEmitted != null)
				{
					EventEmitted(sender, e as IIncomingEvent<T>);
				}
			}
		}
	}
}