using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Pusher
{
	[DataContract]
	public class Event<T> : Event
	{
		private T _dataObject;
		private string _data;

		public T DataObject
		{
			get { return _dataObject; }
			set
			{
				_dataObject = value;
				_data = JsonConvert.SerializeObject(value);
			}
		}

		public override string Data
		{
			get { return _data; }
			set 
			{ 
				_data = value;
				_dataObject = JsonConvert.DeserializeObject<T>(value);
			}
		}

		internal override void FromEvent(Event evt)
		{
			Channel = evt.Channel;
			EventName = evt.EventName;
			Data = evt.Data;
			DataObject = JsonConvert.DeserializeObject<T>(evt.Data);
		}

		public override Type GetDataType()
		{
			return typeof (T);
		}
	}

	[DataContract]
	public class Event : IEvent
	{
		[DataMember(Name = "event")]
		public string EventName { get; set; }

		[DataMember(Name = "channel", EmitDefaultValue = false, IsRequired = false)]
		public string Channel { get; set; }

		[DataMember(Name = "data")]
		public virtual string Data { get; set; }

		internal virtual void FromEvent(Event evt)
		{
		}

		public virtual Type GetDataType()
		{
			return typeof (string);
		}
	}
}