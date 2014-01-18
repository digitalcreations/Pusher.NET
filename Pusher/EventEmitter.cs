using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

		/// <summary>
		/// Wait for a single incoming event of a certain type.
		/// </summary>
		/// <typeparam name="TResult">The type of the value you want to extract from the event</typeparam>
		/// <typeparam name="TEventArgs">The expected event type arguments</typeparam>
		/// <param name="resultDelegate">A function that produces the result from the event data.</param>
		/// <returns>The return value produced by resultDelegate</returns>
		protected async Task<TResult> WaitForSingleEventAsync<TResult, TEventArgs>(
			Func<object, IIncomingEvent<TEventArgs>, TResult> resultDelegate)
		{
			var completionSource = new TaskCompletionSource<TResult>();
			GenericEventEmittedHandler<TEventArgs> eventHandler =
				(sender, e) => completionSource.SetResult(resultDelegate(sender, e));
			var eventSubscription = GetEventSubscription<TEventArgs>();
			eventSubscription.EventEmitted += eventHandler;
			var result = await completionSource.Task;
			eventSubscription.EventEmitted -= eventHandler;
			return result;
		}

		/// <summary>
		/// Wait for a single event of a certain type.
		/// </summary>
		/// <returns>An empty task.</returns>
		protected Task WaitForSingleEventAsync<TEventArgs>()
		{
			return WaitForSingleEventAsync<object, TEventArgs>((sender, e) => null);
		}
	}
}