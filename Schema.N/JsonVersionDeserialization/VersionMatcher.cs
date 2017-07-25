using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Schema.N
{
    public class VersionMatcher : IVersionMatcher
    {
        public int EntityVersion { get; set; }

        public int Weight { get; set; }

        public Func<JObject, bool> EntityMatchesFunc { get; set; }
    }
}
