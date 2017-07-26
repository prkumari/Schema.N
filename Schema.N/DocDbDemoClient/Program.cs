using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using DocumentDbClientLibrary;

namespace DocDbDemoClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var endpoint = ConfigurationManager.AppSettings["endpoint"];
            var authKey = ConfigurationManager.AppSettings["authKey"];
            var database = ConfigurationManager.AppSettings["database"];
            var collection = ConfigurationManager.AppSettings["collection"];
            var documentDbClient = new DocumentDbClient(endpoint, authKey, database, collection);

            Task.Run(async () => await TestVersionUpgradeInDocDb()).Wait();
        }

        private static async  Task TestVersionUpgradeInDocDb()
        {
            throw new System.NotImplementedException();
        }
    }
}
