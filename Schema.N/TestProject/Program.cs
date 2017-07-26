﻿using System;
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

            var entityVersion3Matcher = new VersionMatcher
            {
                EntityVersion = 3,
                EntityMatchesFunc = jObject => jObject.Value<int>("SchemanVersion") == 3,
                Weight = 3
            };

            //entityConversion.VersionDetector = new DefaultEntityVersionDetector("SchemanVersion");
            //entityConversion.RegisterDeserializer("1", new DefaultEntityVersionDeserialization<PersonV1>());
            //entityConversion.RegisterDeserializer("2", new DefaultEntityVersionDeserialization<PersonV2>());

            var transform1 = new JsonTransformRule
            {
                TargetPath = "Id",
                Operation = JsonTransformRuleType.Rename,
                Value = "Identifier"
            };

            var transform2 = new JsonTransformRule
            {
                TargetPath = "Name",
                Operation = JsonTransformRuleType.Rename,
                Value = "FirstName"
            };

            var transform3 = new JsonTransformRule
            {
                TargetPath = "Identifier",
                Operation = JsonTransformRuleType.Rename,
                Value = "Id"
            };

            var transform4 = new JsonTransformRule
            {
                TargetPath = "Description",
                Operation = JsonTransformRuleType.Delete
            };

            var transformerV1V2 = new JsonTransformer(transform1, transform2);
            var transformerV2V3 = new JsonTransformer(transform3, transform4);
            var pocoConverterV1V2 = new JsonTransformerVersionNextConverter<FooV1, FooV2>(transformerV1V2);
            var pocoConverterV2V3 = new JsonTransformerVersionNextConverter<FooV2, FooV3>(transformerV2V3);

            var version1Info = new NewPocoVersionInfo<FooV1>
            {
                NextVersion = 1,
                NextVersionMatcher = entityVersion1Matcher
            };

            var version2Info = new NewPocoVersionInfo<FooV2>()
            {
                NextVersion = 2,
                NextVersionMatcher = entityVersion2Matcher,
                ToNextVersionConverter = pocoConverterV1V2
            };

            var version3Info = new NewPocoVersionInfo<FooV2>()
            {
                NextVersion = 3,
                NextVersionMatcher = entityVersion3Matcher,
                ToNextVersionConverter = pocoConverterV2V3
            };

            entityConversion.RegisterNewVersion(version1Info);
            entityConversion.RegisterNewVersion(version2Info);
            entityConversion.RegisterNewVersion(version3Info);

            var foo1Poco = new FooV1
            {
                Id = 7,
                Name = "Pat",
                Description = "this is Pat."
            };
            var jObject123 = JObject.FromObject(foo1Poco);
            jObject123["SchemanVersion"] = 1;

            var finalResult = entityConversion.DeserializeAndConvertToLatestVersion(jObject123);




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
