using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JsonVersionDeserialization;
using Newtonsoft.Json.Linq;

namespace TestProject
{
    class Program
    {
        public static void Main(string[] args)
        {
            var jsonV1Text = File.ReadAllText(@"C:\hackathon\Schema.N\Schema.N\TestProject\PV1.txt");
            var jsonV2Text = File.ReadAllText(@"C:\hackathon\Schema.N\Schema.N\TestProject\PV2.txt");

            var jtokenV1 = JObject.Parse(jsonV1Text);
            var jtokenV2 = JObject.Parse(jsonV2Text);

            var entityConversion = new JsonToEntityConversion();
            //entityConversion.VersionDetector = new DefaultEntityVersionDetector("SchemanVersion");
            //entityConversion.RegisterDeserializer("1", new DefaultEntityVersionDeserialization<PersonV1>());
            //entityConversion.RegisterDeserializer("2", new DefaultEntityVersionDeserialization<PersonV2>());

            entityConversion.RegisterDefaultDeserializer<PersonV1>("1");
            entityConversion.RegisterDefaultDeserializer<PersonV2>("2");

            var v1Entity = entityConversion.DeserializeJsonToEntityVersion(jtokenV1);
            var v2Entity = entityConversion.DeserializeJsonToEntityVersion(jtokenV2);
        }
    }
}
