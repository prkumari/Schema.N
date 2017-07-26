using System;
using System.Collections.Generic;
using System.IO;
using Schema.N;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using TestProject;

namespace UnitTestProject
{
    [TestClass]
    public class DeserializationTest
    {
        [TestMethod]
        public void VersionDeserializeTest()
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

            var versionDetector = new CompositeVersionDetector(new List<IVersionMatcher> { entityVersion1Matcher, entityVersion2Matcher });

            //entityConversion.VersionDetector = new DefaultEntityVersionDetector("SchemanVersion");
            //entityConversion.RegisterDeserializer("1", new DefaultEntityVersionDeserialization<PersonV1>());
            //entityConversion.RegisterDeserializer("2", new DefaultEntityVersionDeserialization<PersonV2>());

            entityConversion.SetVersionDetector(versionDetector);
            entityConversion.RegisterDefaultDeserializer<PersonV1>(1);
            entityConversion.RegisterDefaultDeserializer<PersonV2>(2);

            var v1Entity = entityConversion.DeserializeJsonToCurrentVersion(jtokenV1);
            var v2Entity = entityConversion.DeserializeJsonToCurrentVersion(jtokenV2);

            Assert.AreEqual(v1Entity.EntityType, typeof (PersonV1));
            Assert.AreEqual(v2Entity.EntityType, typeof (PersonV2));
        }

        [TestMethod]
        public void ConvertValidateBasicOps()
        {
            var jsonV1Text = File.ReadAllText(@"PV1.txt");
            var jsonV2Text = File.ReadAllText(@"PV2.txt");

            var jc = new JsonTransformer();
            var r1 = new JsonTransformRule()
            {
                Operation = JsonTransformRuleType.Rename,
                TargetPath = "Name",
                Value = "awesome"
            };
            var r2 = new JsonTransformRule()
            {
                Operation = JsonTransformRuleType.CopyToken,
                TargetPath = "awesome",
                Value = "SchemanVersion"
            };
            var r3 = new JsonTransformRule()
            {
                Operation = JsonTransformRuleType.Delete,
                TargetPath = "awesome",
            };
            var r4 = new JsonTransformRule()
            {
                Operation = JsonTransformRuleType.NewProperty,
                TargetPath = "Example",
                Value = "AGoodProperty"
            };
            var r5 = new JsonTransformRule()
            {
                Operation = JsonTransformRuleType.SetValue,
                TargetPath = "Example.AGoodProperty",
                Value = "AGoodValue"
            };
            var r6 = new JsonTransformRule()
            {
                Operation = JsonTransformRuleType.SetValue,
                TargetPath = "SchemanVersion",
                Value = new {Test=1, Timestamp="01/01/1970"}
            };

            var rules = new List<JsonTransformRule>();
            rules.Add(r1);
            rules.Add(r2);
            rules.Add(r3);
            rules.Add(r4);
            rules.Add(r5);
            rules.Add(r6);

            var result = jc.ConvertTo(jsonV1Text, jsonV2Text, rules);
            Assert.AreEqual("{\r\n  \"Id\": 1,\r\n  \"FirstName\": \"Priya\",\r\n  " +
                "\"LastName\": \"Kumari\",\r\n  \"DoB\": \"1989-02-01\",\r\n  " +
                "\"SchemanVersion\": {\r\n    \"Test\": 1,\r\n    " +
                "\"Timestamp\": \"01/01/1970\"\r\n  },\r\n  " +
                "\"Example\": {\r\n    \"AGoodProperty\": \"AGoodValue\"\r\n  }\r\n}", result);
        }

        [TestMethod]
        public void ConvertValidateVersionUpgrade()
        {
            var jsonV1Text = File.ReadAllText(@"UserV1.txt");
            var jsonV2Text = File.ReadAllText(@"UserV2.txt");

            var jc = new JsonTransformer();
            var r1 = new JsonTransformRule()
            {
                Operation = JsonTransformRuleType.Rename,
                TargetPath = "about",
                Value = "description"
            };
            var r3 = new JsonTransformRule()
            {
                Operation = JsonTransformRuleType.Delete,
                TargetPath = "eyeColor",
            };

            var rules = new List<JsonTransformRule>();
            rules.Add(r1);
            rules.Add(r3);
            var result = jc.ConvertTo(jsonV1Text, jsonV2Text, rules);
            Assert.AreEqual("{\r\n  \"guid\": \"6557159b-84e8-4651-a6b2-bc57f84c1a7e\",\r\n  \"isActive\": true,\r\n  \"age\": 20,\r\n  \"name\": \"Singleton Craft\",\r\n  \"gender\": \"male\",\r\n  \"address\": \"413 Forrest Street, Lemoyne, Kentucky, 381\",\r\n  \"phone\": \"+1 (971) 407-3748\",\r\n  \"description\": \"Occaecat exercitation consectetur do anim magna nisi sunt. Dolore Lorem ea fugiat velit reprehenderit laboris incididunt cupidatat occaecat velit. Et aliqua nisi nisi amet et velit quis commodo culpa exercitation ad. Labore consectetur ullamco non aliqua ullamco sit do consequat esse nisi labore fugiat. Reprehenderit laboris esse qui laboris amet sint aliquip commodo.\\r\\n\"\r\n}", result);
        }

    }
}
