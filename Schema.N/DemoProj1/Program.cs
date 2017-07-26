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

            var pocoConverterV1V2 =
                new JsonTransformerVersionNextConverter<FooV1, FooV2>(
                    new JsonTransformRule("Id", JsonTransformRuleType.Rename, "Identifier"),
                    new JsonTransformRule("Name", JsonTransformRuleType.Rename, "FirstName"),
                    new JsonTransformRule("SchemanVersion", JsonTransformRuleType.SetValue, 2));

            var pocoConverterV2V3 =
                new JsonTransformerVersionNextConverter<FooV2, FooV3>(
                    new JsonTransformRule("Identifier", JsonTransformRuleType.Rename, "Id"),
                    new JsonTransformRule("Description", JsonTransformRuleType.Delete),
                    new JsonTransformRule("SchemanVersion", JsonTransformRuleType.SetValue, 3));

            entityConversion.RegisterNewVersion(new NewPocoVersionInfo<FooV1>(1));
            entityConversion.RegisterNewVersion(new NewPocoVersionInfo<FooV2>(2, pocoConverterV1V2));
            entityConversion.RegisterNewVersion(new NewPocoVersionInfo<FooV3>(3, pocoConverterV2V3));

            var sampleFoo = GetSampleFooV1();
            var finalResult = entityConversion.DeserializeAndConvertToLatestVersion(sampleFoo);
            var finalPoco = entityConversion.DeserializeJsonToCurrentVersion(finalResult.RawDataObject as JObject);
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
