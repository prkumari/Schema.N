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

            Task.Run(async () => await TestVersionUpgradeInDocDb()).Wait();
        }

        private static async Task TestVersionUpgradeInDocDb()
        {
            var items = await DocDbClient.GetItemsAsync<UserV1>(f => f.isActive || !f.isActive);
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

            foreach (var user in users)
            {
                var latestVersion = entityConversion.DeserializeAndConvertToLatestVersion(user);
                var latestVersionUser = latestVersion.RawDataObject as JObject;
                await DocDbClient.UpdateItemAsync(user["id"].ToString(), latestVersionUser);
            }
        }
    }
}
