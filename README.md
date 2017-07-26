# Schema.N
NoSql DBs are popular in part for the possibility of hosting some "rows" of data in Version1 Schema format alongside other "rows" of data in Version2 Schema format within a single table. This feature enables application upgrades without taking a system outage for DB Schema upgrades. Taking advantage of this feature requires devs to hand-write custom deserialization logic to detect the Schema version of each data row/document.

Schema.N is a reusable utility to parse multiple versions of JSON strings and deserialize into the corresponding versions of POCO objects.

Schema.N Project Philosophy:

The Schema.N philosophy assumes that the dev writes a query to pull a JSON string from any data store (think Azure DocumentDB, Redis, MongoDB, CosmosDB, Azure Table Storage, Kusto, flat files, Apache Kafka, etcetera) into memory, then we detect the schema version of the JSON string, and deserialize into the .NET Data Transfer Object that corresponds to the schema version.

## Sample Usecase:

We want to upgrade the schema from Version 1 to Version 2:

### Version 1 Json Data:
{
    "guid": "6557159b-84e8-4651-a6b2-bc57f84c1a7e",
    "isActive": true,
    "age": 20,
    "eyeColor": "brown",
    "name": "Singleton Craft",
	  "phone": "+1 (971) 407-3748",
    "gender": "male",
    "address": "413 Forrest Street, Lemoyne, Kentucky, 381",
    "about": "Occaecat exercitation consectetur do anim magna nisi sunt. Dolore Lorem ea fugiat velit reprehenderit laboris incididunt cupidatat occaecat velit. Et aliqua nisi nisi amet et velit quis commodo culpa exercitation ad. Labore consectetur ullamco non aliqua ullamco sit do consequat esse nisi labore fugiat. Reprehenderit laboris esse qui laboris amet sint aliquip commodo.\r\n",
}

  
### Version 2 Json Data:
{
    "guid": "6557159b-84e8-4651-a6b2-bc57f84c1a7e",
	  "isActive": true,
    "age": 20,
    "name": "Singleton Craft",
    "gender": "male",
    "address": "413 Forrest Street, Lemoyne, Kentucky, 381",
	  "company": null,
    "phone": "+1 (971) 407-3748",
	  "isActiveEmployee": true,
	  "description": "Occaecat exercitation consectetur do anim magna nisi sunt. Dolore Lorem ea fugiat velit reprehenderit laboris incididunt cupidatat occaecat velit. Et aliqua nisi nisi amet et velit quis commodo culpa exercitation ad. Labore consectetur ullamco non aliqua ullamco sit do consequat esse nisi labore fugiat. Reprehenderit laboris esse qui laboris amet sint aliquip commodo.\r\n",
}


#### Following are the specific changes needs to happen during upgrade:

1. Add new Keys: company, email, phone
2. Rename Keys: about becomes description
3. Copy values to another new key: IsActive to IsActiveEmployee
4. Delete Keys: eyeColor


### Sample Code Snippet:
var jsonV1Text = File.ReadAllText(@"UserV1.txt"); </br>
var jsonV2Text = File.ReadAllText(@"UserV2.txt");</br>

var jc = new JsonTransformer();</br>
var r1 = new JsonTransformRule()</br>
{</br>
	Operation = JsonTransformRuleType.Rename,</br>
	TargetPath = "about",</br>
	Value = "description"</br>
};</br>
var r2 = new JsonTransformRule()</br>
{</br>
	Operation = JsonTransformRuleType.CopyToken,</br>
	TargetPath = "isActive",</br>
	Value = "isActiveEmployee"</br>
};</br>
var r3 = new JsonTransformRule()</br>
{</br>
	Operation = JsonTransformRuleType.Delete,</br>
	TargetPath = "eyeColor",</br>
};</br>

var rules = new List<JsonTransformRule>();</br>
rules.Add(r1);
rules.Add(r2);
rules.Add(r3);</br>
var result = jc.ConvertTo(jsonV1Text, jsonV2Text, rules);</br>
