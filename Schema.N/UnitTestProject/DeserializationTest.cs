using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Schema.N;
using System.Collections.Generic;
using System.IO;
using TestProject;

namespace UnitTestProject
{
	[TestClass]
    public class DeserializationTest
    {
        [TestMethod]
        public void VersionDeserializeTest()
        {
			string jsonV1Text = File.ReadAllText(@"PV1.txt");
			string jsonV2Text = File.ReadAllText(@"PV2.txt");

			var jobjectV1 = JObject.Parse(jsonV1Text);
			var jobjectV2 = JObject.Parse(jsonV2Text);

			var entityConversion = new JsonToEntityConversion();

	        var entityVersion1Matcher = new VersionMatcher(jObject => jObject.Value<int>("SchemanVersion") == 1, 1, 1);
	        var entityVersion2Matcher = new VersionMatcher(jObject => jObject.Value<int>("SchemanVersion") == 2, 2, 2);

			var version1Info = new NewPocoVersionInfo<PersonV1>(1, null, entityVersion1Matcher);
			var version2Info = new NewPocoVersionInfo<PersonV2>(2, null, entityVersion2Matcher);
			entityConversion.RegisterNewVersion(version1Info);
			entityConversion.RegisterNewVersion(version2Info);

			var v1Entity = entityConversion.DeserializeJsonToCurrentVersion(jsonV1Text);
			var v2Entity = entityConversion.DeserializeJsonToCurrentVersion(jsonV2Text);

			Assert.AreEqual(v1Entity.EntityType, typeof(PersonV1));
			Assert.AreEqual(v2Entity.EntityType, typeof(PersonV2));

			v1Entity = entityConversion.DeserializeJsonToCurrentVersion(jobjectV1);
			v2Entity = entityConversion.DeserializeJsonToCurrentVersion(jobjectV2);

			Assert.AreEqual(v1Entity.EntityType, typeof(PersonV1));
			Assert.AreEqual(v2Entity.EntityType, typeof(PersonV2));

		}

		[TestMethod]
        public void TransformValidateBasicOps()
        {
            var jsonV1Text = File.ReadAllText(@"PV1.txt");
            var jsonV2Text = File.ReadAllText(@"PV2.txt");

	        var r1 = new JsonTransformRule("Name", JsonTransformRuleType.Rename, "awesome");
	        var r2 = new JsonTransformRule("awesome", JsonTransformRuleType.CopyToken, "SchemanVersion");
	        var r3 = new JsonTransformRule("awesome", JsonTransformRuleType.Delete);
	        var r4 = new JsonTransformRule("Example", JsonTransformRuleType.NewProperty, "AGoodProperty");
	        var r5 = new JsonTransformRule("Example.AGoodProperty", JsonTransformRuleType.SetValue, "AGoodValue");
	        var r6 = new JsonTransformRule("SchemanVersion", JsonTransformRuleType.SetValue, new { Test = 1, Timestamp = "01/01/1970" });

            var rules = new List<JsonTransformRule>();
            rules.Add(r1);
            rules.Add(r2);
            rules.Add(r3);
            rules.Add(r4);
            rules.Add(r5);
            rules.Add(r6);

            var jc = new JsonTransformer(rules);

            string result = jc.ConvertTo(jsonV1Text, jsonV2Text);
            Assert.AreEqual("{\r\n  \"Identifier\": 1,\r\n  \"FirstName\": " +
                "\"Priya\",\r\n  \"LastName\": \"Kumari\",\r\n  \"DoB\": " +
                "\"1989-02-01\",\r\n  \"SchemanVersion\": {\r\n    " +
                "\"Test\": 1,\r\n    \"Timestamp\": \"01/01/1970\"\r\n  },\r\n  " +
                "\"Example\": {\r\n    \"AGoodProperty\": \"AGoodValue\"\r\n  }\r\n}", result);
        }

        [TestMethod]
        public void TransformValidateVersionUpgrade()
        {
            var jsonV1Text = File.ReadAllText(@"UserV1.txt");
            var jsonV2Text = File.ReadAllText(@"UserV2.txt");

	        var r1 = new JsonTransformRule("about", JsonTransformRuleType.Rename, "description");
	        var r3 = new JsonTransformRule("eyeColor", JsonTransformRuleType.Delete);
            var rules = new List<JsonTransformRule>();
            rules.Add(r1);
            rules.Add(r3);

            var jc = new JsonTransformer(rules);
            var result = jc.ConvertTo(jsonV1Text, jsonV2Text, new JsonLoadSettings() { });
            Assert.AreEqual("{\r\n  \"guid\": \"6557159b-84e8-4651-a6b2-bc57f84c1a7e\",\r\n  \"isActive\": true,\r\n  \"age\": 20,\r\n  \"name\": \"Singleton Craft\",\r\n  \"gender\": \"male\",\r\n  \"address\": \"413 Forrest Street, Lemoyne, Kentucky, 381\",\r\n  \"phone\": \"+1 (971) 407-3748\",\r\n  \"description\": \"Occaecat exercitation consectetur do anim magna nisi sunt. Dolore Lorem ea fugiat velit reprehenderit laboris incididunt cupidatat occaecat velit. Et aliqua nisi nisi amet et velit quis commodo culpa exercitation ad. Labore consectetur ullamco non aliqua ullamco sit do consequat esse nisi labore fugiat. Reprehenderit laboris esse qui laboris amet sint aliquip commodo.\\r\\n\"\r\n}", result);
        }

        [TestMethod]
        public void TransformValidateCustomAction()
        {
            var jsonV1Text = File.ReadAllText(@"UserV1.txt");
            var jsonV2Text = File.ReadAllText(@"UserV2.txt");

            // Example custom function that will transform string type prop values ToUpper()
            var r1 = new JsonTransformRule((json) => 
                {
                    var props = json.Properties();
                    foreach(var prop in props)
                    {
                        if(prop.Value.Type == JTokenType.String)
                        {
                            prop.Value = ((string)prop.Value).ToUpper();
                        }
                    }
                }
            );


            var r2 = new JsonTransformRule("age", JsonTransformRuleType.SetValue, 100);
            var jc = new JsonTransformer(r1, r2);

            var result = jc.ConvertTo(JObject.Parse(jsonV1Text));
            Assert.AreEqual("{\r\n  \"guid\": \"6557159B-84E8-4651-A6B2-BC57F84C1A7E\",\r\n  " +
                "\"isActive\": true,\r\n  \"age\": 100,\r\n  \"eyeColor\": \"BROWN\",\r\n  \"name\": \"SINGLETON CRAFT\",\r\n  " +
                "\"phone\": \"+1 (971) 407-3748\",\r\n  \"gender\": \"MALE\",\r\n  \"address\": \"413 FORREST STREET, LEMOYNE, KENTUCKY, " +
                "381\",\r\n  \"about\": \"OCCAECAT EXERCITATION CONSECTETUR DO ANIM MAGNA NISI SUNT. DOLORE LOREM EA FUGIAT " +
                "VELIT REPREHENDERIT LABORIS INCIDIDUNT CUPIDATAT OCCAECAT VELIT. ET ALIQUA NISI NISI AMET ET VELIT QUIS COMMODO CULPA EXERCITATION AD. " +
                "LABORE CONSECTETUR ULLAMCO NON ALIQUA ULLAMCO SIT DO CONSEQUAT ESSE NISI LABORE FUGIAT. REPREHENDERIT " +
                "LABORIS ESSE QUI LABORIS AMET SINT ALIQUIP COMMODO.\\r\\n\"\r\n}"
                , result.ToString());
        }
    }
}
