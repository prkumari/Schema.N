using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using DocumentDbClientLibrary;
using Newtonsoft.Json.Linq;
using Schema.N;

namespace DocDbDemoClient
{
    class Program
    {
        private static Random random = new Random();

        static DocumentDbClient DocDbClient { get; set; }
        static void Main(string[] args)
        {
            var endpoint = ConfigurationManager.AppSettings["endpoint"];
            var authKey = ConfigurationManager.AppSettings["authKey"];
            var database = ConfigurationManager.AppSettings["database"];
            var collection = ConfigurationManager.AppSettings["collection"];
            DocDbClient = new DocumentDbClient(endpoint, authKey, database, collection);

            while (true)
            {
                Console.WriteLine("===========================");
                Console.WriteLine("Select from the Menu:");
                Console.WriteLine("===========================");
                Console.WriteLine("1. Reset Data");
                Console.WriteLine("2. Upload version 1 Data");
                Console.WriteLine("3. Upgrade from Version 1 to Version 2");
                Console.WriteLine("Choose Option (1/2/3):");
                var ch = Console.ReadLine();
                try
                {
                    switch (ch)
                    {
                        case "1":
                            Task.Run(async () => await ClearTestDB()).Wait();
                            break;
                        case "2":
                            Task.Run(async () => await UploadTestDataToDocDb()).Wait();
                            break;
                        case "3":
                            Task.Run(async () => await TestVersionUpgradeInDocDb()).Wait();
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("================= Exception Occurred!! ===================");
                    Console.WriteLine(e.Message + " StackTrace: " + e.StackTrace);
                    Console.WriteLine("==========================================================");
                }
                Console.WriteLine("Continue? Press y to continue / Press any other key to exit ");
                ch = Console.ReadLine();
                if (string.Compare(ch, "y", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    continue;
                }
                return;
            }            
        }

        private static async Task ClearTestDB()
        {
            // Hacky way to delete
            var itemsV1 = await DocDbClient.GetItemsAsync<UserV1>(f => f.isActive || !f.isActive);
            var itemsV2 = await DocDbClient.GetItemsAsync<UserV2>(f => f.isActive || !f.isActive);
            foreach (var item in itemsV1.ToList())
            {
                await DocDbClient.DeleteItemAsync(item.id);
            }
            try
            {
                foreach (var item in itemsV2.ToList())
                {
                    await DocDbClient.DeleteItemAsync(item.id);
                }
            }
            catch (Exception e)
            {
                // Do nothing, all the resources has been already deleted.
            }
        }

        public static async Task UploadTestDataToDocDb()
        {
            var tasks = Enumerable.Range(0, 10)
                .Select(i => GenerateRandomObject())
                .Select(r => Task.Run(async () => await DocDbClient.CreateItemAsync(r))).ToList();

            Task.WaitAll(tasks.ToArray());
        }

        public static JObject GenerateRandomObject()
        {
            var eyeColors = new[] { "brown", "blue", "green" };
            var firstName = new[] { "Steve", "Joe", "Jeff", "Dan", "Steph", "Jen", "Hilary" };
            var lastName = new[] { "Smith", "Brown", "Peterman", "Clinton" };
            var gender = new[] { "male", "female", "other", "prefer not to specify" };
            var streets = new[] { "Fake St", "Memory Ln", "Microsoft Way" };

            var result = new JObject
            {
                ["guid"] = Guid.NewGuid(),
                ["isActive"] = random.NextDouble() > 0.5,
                ["age"] = random.Next(0, 10),
                ["eyeColor"] = eyeColors[random.Next(0, 2)],
                ["name"] = firstName[random.Next(0, 6)] + " " + lastName[random.Next(0, 3)],
                ["phone"] = $"1-({random.Next(0, 999)})-{random.Next(0, 999)}-{random.Next(0, 9999)}",
                ["gender"] = gender[random.Next(0, 3)],
                ["address"] = $"{random.Next(0, 9999)} {streets[random.Next(0, 2)]}",
                ["about"] = "This is a test record",
                ["SchemanVersion"] = 1
            };

            return result;
        }

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
    }
}
