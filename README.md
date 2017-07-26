# Schema.N
NoSql DBs are popular in part for the possibility of hosting some "rows" of data in Version1 Schema format alongside other "rows" of data in Version2 Schema format within a single table. This feature enables application upgrades without taking a system outage for DB Schema upgrades. Taking advantage of this feature requires devs to hand-write custom deserialization logic to detect the Schema version of each data row/document.

Schema.N is a reusable utility to parse multiple versions of JSON strings and deserialize into the corresponding versions of POCO objects.

Schema.N Project Philosophy:

The Schema.N philosophy assumes that the dev writes a query to pull a JSON string from any data store (think Azure DocumentDB, Redis, MongoDB, CosmosDB, Azure Table Storage, Kusto, flat files, Apache Kafka, etcetera) into memory, then we detect the schema version of the JSON string, and deserialize into the .NET Data Transfer Object that corresponds to the schema version.
 
## Sample method to migrate from old to new version
	static void Foo()
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

## Sample Usecase:

We want to upgrade the schema from Version 1 to Version 2:

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

1. Add new Keys: company, email, phone
2. Rename Keys: about becomes description
3. Copy values to another new key: IsActive to IsActiveEmployee
4. Delete Keys: eyeColor


### Sample Code Snippet:
		public void VersionDeserialize()
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
