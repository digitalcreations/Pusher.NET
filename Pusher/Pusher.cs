using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pusher.Events;
using Pusher.Exceptions;

namespace Pusher
{
	public class Pusher : EventEmitter
	{
		#region Event names

		public const string EventConnectionEstablished = "pusher:connection_established";
		public const string EventSubscribe = "pusher:subscribe";
		private const string InternalEventSubscriptionSucceeded = "pusher_internal:subscription_succeeded";

		#endregion

		private readonly IDictionary<string, Channel> _channels;
		private readonly IConnectionFactory _connectionFactory;
		private readonly IList<IEventContract> _contracts;
		private readonly Options _options;
		private ILogger _logger = new Logger();
		private IConnection _connection;

		#region Constructors

		public Pusher(IConnectionFactory factory, string appKey) 
			: this(factory, appKey, new Options()) { }

		public Pusher(IConnectionFactory factory, string appKey, Options options)
		{
			_connectionFactory = factory;
			_channels = new Dictionary<string, Channel>();
			_contracts = new List<IEventContract>
				{
					EventContract.Create<ConnectionEstablishedEventArgs>(EventConnectionEstablished),
					EventContract.Create<SubscriptionSucceededEventArgs>(InternalEventSubscriptionSucceeded)
				};
			_options = options;
			ApplicationKey = appKey;

			GetEventSubscription<SubscriptionSucceededEventArgs>().EventEmitted += (sender, evt) =>
				{
					if (_channels.ContainsKey(evt.Channel))
					{
						_channels[evt.Channel].Established = true;
					}
				};

			GetEventSubscription<ConnectionEstablishedEventArgs>().EventEmitted += async (sender, evt) =>
				{
					SocketId = evt.DataObject.SocketId;
					foreach (var channel in _channels)
					{
						await SubscribeToChannelAsync(channel.Key, false);
					}
				};
		}

		#endregion

		public string SocketId { get; private set; }
		public string ApplicationKey { get; private set; }

		public ILogger Logger
		{
			get { return _logger; }
			set { _logger = value; }
		}

		public async Task ConnectAsync()
		{
			_connection = _connectionFactory.Create(new Uri(string.Format("{0}://ws.pusherapp.com:{1}/app/{2}?protocol=5", _options.SchemeString, _options.Port,
					                      ApplicationKey)));
			_connection.OnData += ReceivedEvent;
			await _connection.Open();
		}

		public void AddContract(IEventContract contract)
		{
			if (_contracts.Any(c => c.Name == contract.Name))
			{
				throw new EventNameInUseException(contract.Name);
			}

			_contracts.Add(contract);
		}

		private async Task<string> GetSocketIdAsync()
		{
			if (!string.IsNullOrEmpty(SocketId))
			{
				return SocketId;
			}

			// wait for ConnectionEstablishedEvent, and use socket ID
			var completionSource = new TaskCompletionSource<string>();
			GenericEventEmittedHandler<ConnectionEstablishedEventArgs> eventHandler =
				(sender, e) => completionSource.SetResult(e.DataObject.SocketId);
			GetEventSubscription<ConnectionEstablishedEventArgs>().EventEmitted += eventHandler;
			await completionSource.Task;
			GetEventSubscription<ConnectionEstablishedEventArgs>().EventEmitted -= eventHandler;
			return completionSource.Task.Result;
		}

		private void ReceivedEvent(object sender, DataReceivedEventArgs dataReceivedEventArgs)
		{
			_logger.Debug("Parsing event: {0}", dataReceivedEventArgs.TextData);
			var evt = JsonConvert.DeserializeObject<Event>(dataReceivedEventArgs.TextData);
			_logger.Debug("Parsed event {0}", evt.EventName);
			var contracts = _contracts.Where(e => e.Name == evt.EventName).ToList();
			if (contracts.Any())
			{
				_logger.Debug("Found contract for event '{0}'", evt.EventName);
				var contract = contracts.First();
				var type = typeof (Event<>).MakeGenericType(contract.DeserializeAs);
				var newEvt = (Event) Activator.CreateInstance(type);
				newEvt.FromEvent(evt);
				evt = newEvt;
			}
			if (_options.RaiseAllEventsOnPusher || String.IsNullOrEmpty(evt.Channel))
			{
				_logger.Debug("Raising event '{0}' on self", evt.EventName);
				EmitEvent(evt);
			}

			if (!String.IsNullOrEmpty(evt.Channel))
			{
				if (!_channels.ContainsKey(evt.Channel))
				{
					throw new MessageForNonSubscribedChannelException(evt.Channel);
				}
				_logger.Debug("Forwarding event '{0}' to channel '{1}'", evt.EventName, evt.Channel);
				var channel = _channels[evt.Channel];
				channel.EmitEvent(evt);
			}
		}

		internal async Task TriggerEventAsync(IEvent e)
		{
			var json = JsonConvert.SerializeObject(e);
			_logger.Debug("Sending event {0} to {1}: {2}", e.EventName, e.Channel ?? "all channels", json);
			await _connection.SendMessage(json);
		}

		private async Task<Channel> SubscribeToChannelAsync(string channelName, bool checkChannelList)
		{
			if (checkChannelList && _channels.ContainsKey(channelName))
			{
				return _channels[channelName];
			}

			var channel = CreateChannel(channelName);
			var subscribeEvent = new SubscribeEvent(channelName);
			if (channel is PrivateChannel && _options.Authenticator == null)
			{
				throw new AuthenticatorMissingException(channelName);
			}
			if (channel is PrivateChannel)
			{
				var socketId = await GetSocketIdAsync();
				var authentication = await _options.Authenticator.AuthenticateAsync(socketId);
				subscribeEvent.DataObject.Auth = authentication.Auth;
				subscribeEvent.DataObject.ChannelData = authentication.ChannelData;
			}
			if (checkChannelList)
			{
				_channels.Add(channelName, channel);
			}
			await TriggerEventAsync(subscribeEvent);

			if (!(channel is PresenceChannel))
			{
				channel.Established = true;
			}

			return channel;
		}

		public async Task<Channel> SubscribeToChannelAsync(string channelName)
		{
			return await SubscribeToChannelAsync(channelName, true);
		}

		private static Channel CreateChannel(string channelName)
		{
			if (channelName.ToLower().StartsWith("private-"))
			{
				return new PrivateChannel(channelName);
			}
			if (channelName.ToLower().StartsWith("presence-"))
			{
				return new PresenceChannel(channelName);
			}

			return new Channel(channelName);
		}

		public void Disconnect()
		{
			_connection.OnData -= ReceivedEvent;
			_connection.Close();
			_connection = null;
		}
	}
}