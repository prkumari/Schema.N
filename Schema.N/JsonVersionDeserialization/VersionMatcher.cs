using Newtonsoft.Json.Linq;
using System;

namespace Schema.N
{
	public class VersionMatcher : IVersionMatcher
    {
		public VersionMatcher() {}

		public VersionMatcher(Func<JObject, bool> entityMatchesFunc, int entityVersion, int weight)
	    {
		    EntityMatchesFunc = entityMatchesFunc;
		    EntityVersion = entityVersion;
		    Weight = weight;
	    }

		public Func<JObject, bool> EntityMatchesFunc { get; set; }

		public int EntityVersion { get; set; }

        public int Weight { get; set; }

    }
}
