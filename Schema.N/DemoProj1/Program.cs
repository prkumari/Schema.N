using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Schema.N;
using TestProject;

namespace DemoProj1
{
    class Program
    {
        static void Main(string[] args)
        {
            var entityConversion = new JsonToEntityConversion();

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
                NextVersion = 1
            };

            var version2Info = new NewPocoVersionInfo<FooV2>()
            {
                NextVersion = 2,
                ToNextVersionConverter = pocoConverterV1V2
            };

            var version3Info = new NewPocoVersionInfo<FooV2>()
            {
                NextVersion = 3,
                ToNextVersionConverter = pocoConverterV2V3
            };

            entityConversion.RegisterNewVersion(version1Info);
            entityConversion.RegisterNewVersion(version2Info);
            entityConversion.RegisterNewVersion(version3Info);

            var sampleFoo = GetSampleFooV1();
            var finalResult = entityConversion.DeserializeAndConvertToLatestVersion(sampleFoo);

        }

        private static JObject GetSampleFooV1()
        {
            var foo1Poco = new FooV1
            {
                Id = 7,
                Name = "Pat",
                Description = "this is Pat."
            };
            var jObject = JObject.FromObject(foo1Poco);
            jObject["SchemanVersion"] = 1;
            return jObject;
        }
    }
}
