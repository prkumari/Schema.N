using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace JsonVersionDeserialization
{
    public class JsonToEntityConversion
    {
        public IReadOnlyDictionary<string, IEntityVersionDeserialization> ReadOnlyVersionToDeserializerRegistry => VersionToDeserializerRegistry;

        public IEntityVersionDetector VersionDetector { get; set; }

        private Dictionary<string, IEntityVersionDeserialization> VersionToDeserializerRegistry { get; }

        public JsonToEntityConversion()
        {
            VersionDetector = new DefaultEntityVersionDetector("SchemanVersion");
            VersionToDeserializerRegistry = new Dictionary<string, IEntityVersionDeserialization>();
        }

        public void RegisterDeserializer(string id, IEntityVersionDeserialization deserializer)
        {
            VersionToDeserializerRegistry[id] = deserializer;
        }

        public void RegisterDefaultDeserializer<TEntity>(string id)
        {
            VersionToDeserializerRegistry[id] = new DefaultEntityVersionDeserialization<TEntity>();
        }

        public void SetVersionDetector(IEntityVersionDetector versionDetector)
        {
            VersionDetector = versionDetector;
        }

        public object DeserializeJsonToEntityVersion(JToken json)
        {
            var version = VersionDetector.GetEntityVersion(json);
            var deserializer = VersionToDeserializerRegistry[version];
            return deserializer.Deserialize(json);
        }
    }
}
