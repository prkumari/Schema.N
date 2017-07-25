using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Schema.N
{
    public class CompositeVersionDetector : IEntityVersionDetector
    {
        public IReadOnlyList<IVersionMatcher> ReadOnlyVersionMatchers => VersionMatchers;
         
        private List<IVersionMatcher> VersionMatchers { get; set; }

        public CompositeVersionDetector(List<IVersionMatcher> versionMatchers)
        {
            if (versionMatchers.GroupBy(v => v.EntityVersion).Any(g => g.Count() > 1))
            {
                throw new ArgumentException("Can not have multiple version matchers that reference the same version id.");
            }

            // Sort them by rank so that we can iterate and return first success.
            // Note: big should outrank small.
            VersionMatchers = new List<IVersionMatcher>(versionMatchers.OrderByDescending(m => m.Weight));
        }

        public int GetEntityVersion(JObject json)
        {
            foreach (var matcher in VersionMatchers)
            {
                if (matcher.EntityMatchesFunc(json))
                {
                    return matcher.EntityVersion;
                }
            }

            // TODO better exception or perhaps ignore this bad boy, or better response type.
            throw new Exception("No version matcher matched this entity.");
        }
    }
}
