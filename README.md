# Schema.N

nuget package available here:
https://www.nuget.org/packages/Schema.N/

NoSql DBs are popular in part for the possibility of hosting some "rows" of data in Version1 Schema format alongside other "rows" of data in Version2 Schema format within a single table. This feature enables application upgrades without taking a system outage for DB Schema upgrades. Taking advantage of this feature requires devs to hand-write custom deserialization logic to detect the Schema version of each data row/document.

Schema.N is a reusable utility to parse multiple versions of JSON strings and deserialize into the corresponding versions of POCO objects.

Schema.N Project Philosophy:

The Schema.N philosophy assumes that the dev writes a query to pull a JSON string from any data store (think Azure DocumentDB, Redis, MongoDB, CosmosDB, Azure Table Storage, Kusto, flat files, Apache Kafka, etcetera) into memory, then we detect the schema version of the JSON string, and deserialize into the .NET Data Transfer Object that corresponds to the schema version.
 
## Sample use case - deserialize JSON into the appropriate POCO

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Schema.N;
using System;

namespace ConsoleApp1
{
	class Program
	{
		static void Main(string[] args)
		{
			string jsonV1 = "{ Foo: 5, Bar: 'Hello World' }";

			//this works great (before you upgrade your app/service to version 2)
			PocoV1 pocoV1 = JsonConvert.DeserializeObject<PocoV1>(jsonV1);

			try
			{
				PocoV2 pocoV2 = JsonConvert.DeserializeObject<PocoV2>(jsonV1);
				Console.WriteLine("This code is unreachable.");
			}
			catch (Newtonsoft.Json.JsonReaderException)
			{
				Console.WriteLine("Some JSON format changes cause runtime exceptions when parsing older JSON strings into the latest POCO.");
			}
			
			var entityConversion = new JsonToEntityConversion();
			JToken jToken;
			//The absence of a "Version" property means we're dealing with version 1 JSON format.
			var entityVersion1Matcher = new VersionMatcher(jObject => false == jObject.TryGetValue("Version", out jToken), 1, 1);
			//If a "Version" attribute exists, and the value = 2, then we're dealing with a version 2 JSON format
			var entityVersion2Matcher = new VersionMatcher(jObject => jObject.Value<int>("Version") == 2, 2, 2);
			var version1Info = new NewPocoVersionInfo<PocoV1>(1, null, entityVersion1Matcher);
			var version2Info = new NewPocoVersionInfo<PocoV2>(2, null, entityVersion2Matcher);
			entityConversion.RegisterNewVersion(version1Info);
			entityConversion.RegisterNewVersion(version2Info);
			
			var v1Entity = entityConversion.DeserializeJsonToCurrentVersion(jsonV1);
			if(typeof(PocoV1) == v1Entity.EntityType)
				Console.WriteLine("Schema.N succesfully determined the correct JSON format and deserialized into the correct .NET class (PocoV1)");

			//Output:
			//Some JSON format changes cause runtime exceptions when parsing older JSON strings into the latest POCO.
			//Schema.N succesfully determined the correct JSON format and deserialized into the correct .NET class (PocoV1)
		}
	}

	public class PocoV1
	{
		public int Foo;
		public string Bar;
	}

	public class PocoV2
	{
		public int Foo;
		public int Bar;
		public int version;
	}
}

## Sample Usecase: Upgrade/Migrate JObject from Version 1 to Version 2:

### Version 1 Json Data:
{</br>
    "guid": "6557159b-84e8-4651-a6b2-bc57f84c1a7e",</br>
    "isActive": true,</br>
    "age": 20,</br>
    "eyeColor": "brown",</br>
    "name": "Singleton Craft",</br>
    "phone": "+1 (971) 407-3748",</br>
    "gender": "male",</br>
    "address": "413 Forrest Street, Lemoyne, Kentucky, 381",</br>
    "about": "Occaecat exercitation consectetur do anim magna nisi sunt. Dolore Lorem ea fugiat velit reprehenderit laboris incididunt cupidatat occaecat velit. Et aliqua nisi nisi amet et velit quis commodo culpa exercitation ad. Labore consectetur ullamco non aliqua ullamco sit do consequat esse nisi labore fugiat. Reprehenderit laboris esse qui laboris amet sint aliquip commodo.\r\n",</br>
}

  
### Version 2 Json Data:
{</br>
    "guid": "6557159b-84e8-4651-a6b2-bc57f84c1a7e",</br>
    "isActive": true,</br>
    "age": 20,</br>
    "name": "Singleton Craft",</br>
    "gender": "male",</br>
    "address": "413 Forrest Street, Lemoyne, Kentucky, 381",</br>
    "company": null,</br>
    "phone": "+1 (971) 407-3748",</br>
    "isActiveEmployee": true,</br>
    "description": "Occaecat exercitation consectetur do anim magna nisi sunt. Dolore Lorem ea fugiat velit reprehenderit laboris incididunt cupidatat occaecat velit. Et aliqua nisi nisi amet et velit quis commodo culpa exercitation ad. Labore consectetur ullamco non aliqua ullamco sit do consequat esse nisi labore fugiat. Reprehenderit laboris esse qui laboris amet sint aliquip commodo.\r\n",</br>
}


#### Following are the specific changes needs to happen during upgrade:

1. Add new Keys: company, email
2. Rename Keys: about becomes description
3. Copy values to another new key: IsActive to IsActiveEmployee
4. Delete Keys: eyeColor

Using DocumentDB as a client to test the version migration.

### Sample Code Snippet:
	private static async Task TestVersionUpgradeInDocDb()
        {
            var items = await DocDbClient.GetItemsAsync<UserV1>(f => f.SchemanVersion == 1);
            var users = items.ToList().Select(user => JObject.FromObject(user)).ToList();
            var entityConversion = new JsonToEntityConversion();

            var pocoConverterV1V2 =
                new JsonTransformerVersionNextConverter<UserV1, UserV2>(
                    new JsonTransformRule("about", JsonTransformRuleType.Rename, "description"),
                    new JsonTransformRule("isActiveEmployee", JsonTransformRuleType.NewProperty),
                    new JsonTransformRule("isActive", JsonTransformRuleType.CopyToken, "isActiveEmployee"),
                    new JsonTransformRule("company", JsonTransformRuleType.NewProperty),
                    new JsonTransformRule("email", JsonTransformRuleType.NewProperty),
                    new JsonTransformRule("eyeColor", JsonTransformRuleType.Delete),
                    new JsonTransformRule("SchemanVersion", JsonTransformRuleType.SetValue, 2));

            entityConversion.RegisterNewVersion(new NewPocoVersionInfo<UserV1>(1));
            entityConversion.RegisterNewVersion(new NewPocoVersionInfo<UserV2>(2, pocoConverterV1V2));

            var upgradedVersionUsers = users.Select(user => 
                entityConversion.DeserializeAndConvertToLatestVersion(user).RawDataObject as JObject).ToList();

            foreach (var user in upgradedVersionUsers)
            {
                await DocDbClient.UpdateItemAsync(user["id"].ToString(), user);
            }
        }
