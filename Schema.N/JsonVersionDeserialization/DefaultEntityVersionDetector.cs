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

        public string GetEntityVersion(JToken json)
        {
            return json.Value<int?>(VersionKey)?.ToString();
        }
    }
}
