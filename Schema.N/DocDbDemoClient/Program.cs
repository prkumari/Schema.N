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
            foreach (var item in itemsV2.ToList())
            {
                await DocDbClient.DeleteItemAsync(item.id);
            }
        }

        public static async Task UploadTestDataToDocDb()
        {
            return;
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
