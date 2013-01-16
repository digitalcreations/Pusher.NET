using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Pusher.Events
{
	[DataContract]
	public class PresenceData
	{
		[DataMember(Name = "ids")]
		public IEnumerable<string> Ids { get; set; }

		[DataMember(Name = "count")]
		public int Count { get; set; }

		[DataMember(Name = "hash")]
		public IDictionary<string, object> Hash { get; set; }
	}
}