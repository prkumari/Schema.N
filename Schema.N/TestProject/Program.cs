using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentDbClientLibrary;
using JsonVersionDeserialization;
using Newtonsoft.Json.Linq;

namespace TestProject
{
    class Program
    {
        public static void Main(string[] args)
        {
            //ConvertV1ToV2();
            Task.Run(async() => await DocumentDBTesting()).Wait();
        }

        private static async Task DocumentDBTesting()
        {
            var endpoint = ConfigurationManager.AppSettings["endpoint"];
            var authKey = ConfigurationManager.AppSettings["authKey"];
            var database = ConfigurationManager.AppSettings["database"];
            var collection = ConfigurationManager.AppSettings["collection"];

            var client = new DocumentDbClient(endpoint, authKey, database, collection);

            var jsonV1Text = File.ReadAllText(@"C:\hackathon\Schema.N\Schema.N\TestProject\PV1.txt");

            var jtokenV1 = JObject.Parse(jsonV1Text);
            var item1 = await client.CreateItemAsync(jtokenV1);
            var getItems = await client.GetItemsAsync<PersonV1>(f => f.Name== "Priya Kumari");
        }

        private static void ConvertV1ToV2()
        {
            var jsonV1Text = File.ReadAllText(@"C:\hackathon\Schema.N\Schema.N\TestProject\PV1.txt");
            var jsonV2Text = File.ReadAllText(@"C:\hackathon\Schema.N\Schema.N\TestProject\PV2.txt");

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

            var v1Entity = entityConversion.DeserializeJsonToEntityVersion(jtokenV1);
            var v2Entity = entityConversion.DeserializeJsonToEntityVersion(jtokenV2);
        }
    }
}
