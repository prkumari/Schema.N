using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Schema.N
{
    public class JsonToEntityConversion
    {
        public IReadOnlyDictionary<int, IEntityVersionDeserialization> ReadOnlyVersionToDeserializerRegistry => VersionToDeserializerRegistry;

        public IEntityVersionDetector VersionDetector { get; set; }

        private Dictionary<int, IEntityVersionDeserialization> VersionToDeserializerRegistry { get; }

        public JsonToEntityConversion()
        {
            VersionDetector = new DefaultEntityVersionDetector("SchemanVersion");
            VersionToDeserializerRegistry = new Dictionary<int, IEntityVersionDeserialization>();
        }

        public void RegisterDeserializer(int id, IEntityVersionDeserialization deserializer)
        {
            VersionToDeserializerRegistry[id] = deserializer;
        }

        public void RegisterDefaultDeserializer<TEntity>(int id)
        {
            VersionToDeserializerRegistry[id] = new DefaultEntityVersionDeserialization<TEntity>();
        }

        public void SetVersionDetector(IEntityVersionDetector versionDetector)
        {
            VersionDetector = versionDetector;
        }

        public IVersionResponseWrapper<object> DeserializeJsonToEntityVersion(JObject json)
        {
            var version = VersionDetector.GetEntityVersion(json);
            var deserializer = VersionToDeserializerRegistry[version];

            var response = new VersionResponseWrapper<object>
            {
                Entity = deserializer.Deserialize(json),
                EntityVersion = version,
                EntityType = deserializer.GetEntityType()
            };

            return response;
        }
    }
}
