using System;
using Newtonsoft.Json.Linq;

namespace JsonVersionDeserialization
{
    public class DefaultEntityVersionDetector : IEntityVersionDetector
    {
        public string VersionKey { get; set; }

        public DefaultEntityVersionDetector(string versionKey)
        {
            VersionKey = versionKey;
        }

        public int GetEntityVersion(JObject json)
        {
            var entityVersion = json.Value<int?>(VersionKey);

            if (!entityVersion.HasValue)
            {
                throw new ArgumentException($"Json provided did not have {VersionKey} property that was a valid int.");
            }

            return entityVersion.Value;
        }
    }
}
