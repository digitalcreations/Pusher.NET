using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Newtonsoft.Json;
using Pusher.Events;
using Pusher.Exceptions;

namespace Pusher.Tests.WindowsStore
{
	[TestClass]
	public class PusherTest
	{
		class FakeConnection : IConnection
		{
			public void Close()
			{
				if (OnClose != null)
				{
					OnClose(this, new EventArgs());
				}
			}

			public async Task Open()
			{
				if (OnOpen != null)
				{
					OnOpen(this, new EventArgs());
				}
				SendData(JsonConvert.SerializeObject(new Event
					{
						EventName = Pusher.EventConnectionEstablished,
						Data = "{\"socket_id\":\"a\"}"
					}));
			}

			public async Task SendMessage(string data)
			{
				var evt = JsonConvert.DeserializeObject<Event>(data);
				if (evt.EventName == Pusher.EventSubscribe)
				{
					var evt2 = JsonConvert.DeserializeObject<SubscribeEvent>(data);
					SendData(@"{""event"":""pusher_internal:subscription_succeeded"",""data"":""{\""presence\"":{\""count\"":1,\""ids\"":[\""pusher-test\""],\""hash\"":{\""pusher-test\"":null}}}"",""channel"":""" + evt2.DataObject.Channel + @"""}");
				}
			}

			public void SendData(string data)
			{
				if (OnData != null)
				{
					OnData(this, new DataReceivedEventArgs { TextData = data });
				}
			}

			public event EventHandler<EventArgs> OnClose;
			public event EventHandler<EventArgs> OnOpen;
			public event EventHandler<DataReceivedEventArgs> OnData;
		}

		class FakeConnectionFactory : IConnectionFactory
		{
			public FakeConnection LastCreated { get; private set; }

			public IConnection Create(Uri endpoint)
			{
				LastCreated = new FakeConnection();
				return LastCreated;
			}
		}

		class FakeAuthenticator : IAuthenticator
		{
			public async Task<IAuthenticationData> AuthenticateAsync(string socketId)
			{
				return new AuthenticationData
					{
						Auth = "foo123",
						ChannelData = new object()
					};
			}
		}

		[DataContract]
		class FakeEvent
		{
			[DataMember(Name = "id")]
			public int Id { get; set; }

			[DataMember(Name = "text")]
			public string Text { get; set; }
		}

		[TestMethod]
		public async Task TestPusherEventsAsync()
		{
			var factory = new FakeConnectionFactory();
			var pusher = new Pusher(factory, "abcd1234", new Options { RaiseAllEventsOnPusher = true });

			var events = new List<Event>();

			pusher.GetEventSubscription<ConnectionEstablishedEventArgs>().EventEmitted += (sender, evt) =>
				{
					Assert.AreEqual(evt.DataObject.SocketId, "a");
					Assert.AreEqual(evt.EventName, "pusher:connection_established");
					events.Add(evt);
				};
			await pusher.ConnectAsync();

			Assert.AreEqual(1, events.Count);
			events.Clear();

			pusher.GetEventSubscription<SubscriptionSucceededEventArgs>().EventEmitted += (sender, evt) =>
			{
				Assert.AreEqual(evt.Channel, "foo");
				Assert.AreEqual(evt.EventName, "pusher_internal:subscription_succeeded");
				events.Add(evt);
			};

			await pusher.SubscribeToChannelAsync("foo");

			Assert.AreEqual(1, events.Count);
		}

		[TestMethod]
		public async Task TestRaiseAllEventsOnPusherAsync()
		{
			var factory = new FakeConnectionFactory();
			var pusher = new Pusher(factory, "abcd1234", new Options { RaiseAllEventsOnPusher = false });

			var eventsOnPusher = 0;

			pusher.EventEmitted += (sender, evt) => eventsOnPusher++;
			await pusher.ConnectAsync();

			Assert.AreEqual(1, eventsOnPusher);

			var eventsOnChannel = 0;
			var channel = await pusher.SubscribeToChannelAsync("foo");
			channel.EventEmitted += (sender, evt) => eventsOnChannel++;

			// the subscribe successful event is raised on channel, but it is raised before we can hook up the event
			Assert.AreEqual(0, eventsOnChannel);
			// RaiseAllEventsOnPusher prevents the subscribe successful event from being raised on pusher
			Assert.AreEqual(1, eventsOnPusher);
		}

		[TestMethod]
		public async Task TestMissingAuthenticatorThrowsExceptionAsync()
		{
			var factory = new FakeConnectionFactory();
			var pusher = new Pusher(factory, "abcd1234");
			await pusher.ConnectAsync();

			Assert.ThrowsException<AggregateException>(() => pusher.SubscribeToChannelAsync("private-foo").Wait());
		}

		[TestMethod]
		public async Task TestChannelTypesAsync()
		{
			var factory = new FakeConnectionFactory();
			var pusher = new Pusher(factory, "abcd1234", new Options() { Authenticator = new FakeAuthenticator() });
			await pusher.ConnectAsync();

			var privateChannel = await pusher.SubscribeToChannelAsync("private-foo");
			var presenceChannel = await pusher.SubscribeToChannelAsync("presence-foo");
			var normalChannel = await pusher.SubscribeToChannelAsync("foo");

			Assert.AreEqual(typeof(PrivateChannel), privateChannel.GetType());
			Assert.AreEqual(typeof(PresenceChannel), presenceChannel.GetType());
			Assert.AreEqual(typeof(Channel), normalChannel.GetType());
		}

		[TestMethod]
		public async Task TestEventContractAsync()
		{
			var factory = new FakeConnectionFactory();
			var pusher = new Pusher(factory, "abcd1234", new Options() { Authenticator = new FakeAuthenticator() });
			pusher.AddContract(EventContract.Create<FakeEvent>("fooHappened"));
			await pusher.ConnectAsync();

			var sentEvent = new Event<FakeEvent>
				{
					Channel = "foo",
					DataObject = new FakeEvent {Id = 1, Text = "foo"},
					EventName = "fooHappened"
				};
			var channel = await pusher.SubscribeToChannelAsync("foo");
			var eventsReceived = 0;
			channel.GetEventSubscription<FakeEvent>().EventEmitted += (sender, receivedEvent) =>
				{
					Assert.AreEqual(sentEvent.DataObject.Id, receivedEvent.DataObject.Id);
					Assert.AreEqual(sentEvent.DataObject.Text, receivedEvent.DataObject.Text);
					Assert.AreEqual(sentEvent.EventName, receivedEvent.EventName);
					Assert.AreEqual(sentEvent.Channel, receivedEvent.Channel);
					eventsReceived++;
				};

			channel.EventEmitted += (sender, receivedEvent) =>
				{
					Assert.AreEqual(sentEvent.EventName, receivedEvent.EventName);
					Assert.AreEqual(sentEvent.Channel, receivedEvent.Channel);
					eventsReceived++;

					Assert.AreEqual(typeof(Event<FakeEvent>), receivedEvent.GetType());
				};

			factory.LastCreated.SendData(JsonConvert.SerializeObject(sentEvent));
			Assert.AreEqual(2, eventsReceived);
		}
	}
}
