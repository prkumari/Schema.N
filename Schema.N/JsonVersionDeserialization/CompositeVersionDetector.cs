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

        public CompositeVersionDetector()
        {
            VersionMatchers = new List<IVersionMatcher>();
        }

        // todo this must be called in the correct order.
        public void AddVersionMatcher(IVersionMatcher matcher)
        {
            if (VersionMatchers.Any(m => m.EntityVersion == matcher.EntityVersion))
            {
                throw new ArgumentException("Can not have multiple version matchers that reference the same version id.");
            }

            VersionMatchers.Add(matcher);
        }

        public int GetEntityVersion(JObject json)
        {
            var orderedMatchers = VersionMatchers.OrderBy(m => m.Weight);

            foreach (var matcher in orderedMatchers)
            {
                if (matcher.EntityMatchesFunc(json))
                {
                    return matcher.EntityVersion;
                }
            }

            var defaultMatch = DefaultMatch(json);
            if (defaultMatch.HasValue)
            {
                return defaultMatch.Value;
            }

            // TODO better exception or perhaps ignore this bad boy, or better response type.
            throw new Exception("No version matcher matched this entity.");
        }

        private int? DefaultMatch(JObject json)
        {
            var schemanVersion = json.Value<int?>("SchemanVersion");
            return schemanVersion;
        }
    }
}
