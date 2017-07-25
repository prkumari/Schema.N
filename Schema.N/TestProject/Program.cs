using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentDbClientLibrary;
using Newtonsoft.Json.Linq;
using Schema.N;

namespace TestProject
{
    class Program
    {
        public static void Main(string[] args)
        {
            var intsList = new List<int> {1, 2, 3, 4, 5};

            var x1 = intsList.OfType<int?>().FirstOrDefault(x => x == 3);
            var x2 = intsList.OfType<int?>().FirstOrDefault(x => x == 7);

            var jsonV1Text = File.ReadAllText(@"E:\Hackathon\Git\Schema.N\Schema.N\TestProject\PV1.txt");
            var jsonV2Text = File.ReadAllText(@"E:\Hackathon\Git\Schema.N\Schema.N\TestProject\PV2.txt");

            var jtokenV1 = JObject.Parse(jsonV1Text);
            var jtokenV2 = JObject.Parse(jsonV2Text);

            var entityConversion = new JsonToEntityConversion();

            var entityVersion1Matcher = new VersionMatcher
            {
                EntityVersion = 1,
                EntityMatchesFunc = jObject => jObject.Value<int>("SchemanVersion") == 1,
                Weight = 1
            };

            var entityVersion2Matcher = new VersionMatcher
            {
                EntityVersion = 2,
                EntityMatchesFunc = jObject => jObject.Value<int>("SchemanVersion") == 2,
                Weight = 2
            };

            var versionDetector =
                new CompositeVersionDetector(new List<IVersionMatcher> {entityVersion1Matcher, entityVersion2Matcher});

            //entityConversion.VersionDetector = new DefaultEntityVersionDetector("SchemanVersion");
            //entityConversion.RegisterDeserializer("1", new DefaultEntityVersionDeserialization<PersonV1>());
            //entityConversion.RegisterDeserializer("2", new DefaultEntityVersionDeserialization<PersonV2>());

            entityConversion.SetVersionDetector(versionDetector);
            entityConversion.RegisterDefaultDeserializer<PersonV1>(1);
            entityConversion.RegisterDefaultDeserializer<PersonV2>(2);

            entityConversion.RegisterVersionConversion<PersonV1, PersonV2>(1, 2, ConvertPersonV1ToV2);
            entityConversion.RegisterVersionConversion<PersonV2, PersonV3>(2, 3, ConvertPersonV2ToV3);

            var v1Entity = entityConversion.DeserializeJsonToCurrentVersion(jtokenV1);

            var finalResult = entityConversion.DeserializeAndConvertToLatestVersion(jtokenV1);




            var v2Entity = entityConversion.DeserializeJsonToCurrentVersion(jtokenV2);
        }

        private static DataVersionInfo<JObject, PersonV2> ConvertPersonV1ToV2(DataVersionInfo<JObject, PersonV1> startData)
        {
            var namesplits = startData.Poco.Name.Split(' ');

            var newperson = new PersonV2
            {
                Identifier = startData.Poco.Identifier,
                FirstName = namesplits[0],
                LastName = namesplits.Length > 1 ? string.Join(" ", namesplits.Skip(1)) : null,
                DoB = null
            };

            return new DataVersionInfo<JObject, PersonV2>(JObject.FromObject(newperson), newperson);
        }

        private static DataVersionInfo<JObject, PersonV3> ConvertPersonV2ToV3(DataVersionInfo<JObject, PersonV2> startData)
        {
            var newperson = new PersonV3
            {
                Identifier = startData.Poco.Identifier,
                FirstName = startData.Poco.FirstName,
                LastName = startData.Poco.LastName,
                DoB = startData.Poco.DoB,
                PowerLevel = null
            };

            return new DataVersionInfo<JObject, PersonV3>(JObject.FromObject(newperson), newperson);
        }
    }
}
