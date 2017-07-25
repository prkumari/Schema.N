using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace JsonVersionDeserialization
{
    public class JsonToEntityConversion
    {
        public IReadOnlyDictionary<int, IEntityVersionDeserialization> ReadOnlyVersionToDeserializerRegistry => VersionToDeserializerRegistry;

        public IEntityVersionDetector VersionDetector { get; set; }

        private Dictionary<int, IEntityVersionDeserialization> VersionToDeserializerRegistry { get; }

        private Dictionary<Tuple<int, int>, IVersionNextConverterTypeless> VersionConverters { get; set; }

        public JsonToEntityConversion()
        {
            VersionDetector = new DefaultEntityVersionDetector("SchemanVersion");
            VersionToDeserializerRegistry = new Dictionary<int, IEntityVersionDeserialization>();
            VersionConverters = new Dictionary<Tuple<int, int>, IVersionNextConverterTypeless>();
        }

        public void RegisterVersionConversion<TPocoThis, TPocoNext>(int startVersion, int endVersion,
            Func<DataVersionInfo<JObject, TPocoThis>, DataVersionInfo<JObject, TPocoNext>> conversionFunc)
        {
            VersionConverters[Tuple.Create(startVersion, endVersion)] =
                new AbstractVersionNextConverter<JObject, TPocoThis, TPocoNext>(conversionFunc);
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

        public object DeserializeAndConvertToLatestVersion(JObject json)
        {
            var sortedVersions =
                VersionConverters.Keys.SelectMany(t => new List<int> {t.Item1, t.Item2}).Distinct().OrderBy(k => k);

            var deserializeResponse = DeserializeJsonToCurrentVersion(json);
            var curVersion = deserializeResponse.EntityVersion;
            var curPoco = deserializeResponse.Entity;
            var nextVersion = sortedVersions.OfType<int?>().FirstOrDefault(v => v > curVersion);

            //IDataVersionInfo curData = new DataVersionInfo<JObject, TPocoStartType>(json, curPoco);
            IDataVersionInfo curData = new DataVersionInfoUgly<JObject>(json, curPoco, deserializeResponse.EntityType);

            while (nextVersion != null)
            {
                var versionTuple = Tuple.Create(curVersion, nextVersion.Value);
                if (!VersionConverters.ContainsKey(versionTuple))
                {
                    throw new Exception($"Could not find converter from entity version {curVersion} to {nextVersion}.");
                }

                var converter = VersionConverters[versionTuple];
                curData = converter.ConvertToNext(curData);
                curVersion = nextVersion.Value;
                // TODO inefficient
                nextVersion = sortedVersions.OfType<int?>().FirstOrDefault(v => v > curVersion);
            }

            return curData;
        }

        public IVersionResponseWrapper<object> DeserializeJsonToCurrentVersion(JObject json)
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
