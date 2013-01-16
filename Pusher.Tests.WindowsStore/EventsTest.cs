using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace Pusher.Tests.WindowsStore
{
    [TestClass]
    public class EventsTest
    {
		class EventSource : EventEmitter
		{
			public new void EmitEvent(Event e)
			{
				base.EmitEvent(e);
			}
		}

		class EventArgs
		{
			public int Id { get; set; }
		}

        [TestMethod]
        public void TestEventsAreTriggered()
        {
	        var a = new EventSource();
	        var e = new Event {Channel = "a", Data = "{}", EventName = "foo"};

			var events = new List<Event>();

	        a.EventEmitted += (sender, evt) =>
		        {
			        Assert.AreEqual(evt.Channel, e.Channel);
			        Assert.AreEqual(evt.Data, e.Data);
			        Assert.AreEqual(evt.EventName, e.EventName);
			        events.Add(evt);
		        };

			a.EmitEvent(e);
			Assert.AreEqual(events.Count, 1);
        }

		[TestMethod]
		public void TestGenericEvents()
		{
			var a = new EventSource();
			var e = new Event<EventArgs> { Channel = "a", DataObject = new EventArgs { Id = 10 }, EventName = "foo" };
			(e as Event).Data = "{id:10}";

			var events = new List<Event>();

			a.EventEmitted += (sender, evt) =>
				{
					Assert.AreEqual(evt.Channel, e.Channel);
					Assert.AreEqual(evt.Data, e.Data);
					Assert.AreEqual(evt.EventName, e.EventName);
					events.Add(evt);
				};

			a.GetEventSubscription<EventArgs>().EventEmitted += (sender, evt) =>
			{
				Assert.AreEqual(evt.Channel, e.Channel);
				Assert.AreEqual(evt.Data, e.Data);
				Assert.AreEqual(evt.DataObject.Id, e.DataObject.Id);
				Assert.AreEqual(evt.EventName, e.EventName);
				events.Add(evt);
			};

			a.EmitEvent(e);
			Assert.AreEqual(events.Count, 2, "Event should get through twice when you have two subscriptions");
		}


    }
}
