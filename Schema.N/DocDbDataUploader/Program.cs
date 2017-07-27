using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using DocumentDbClientLibrary;
using Newtonsoft.Json.Linq;

namespace DocDbDataUploader
{
    public class Program
    {
        public static Random random = new Random();

        public static void Main(string[] args)
        {
            var endpoint = ConfigurationManager.AppSettings["endpoint"];
            var authKey = ConfigurationManager.AppSettings["authKey"];
            var database = ConfigurationManager.AppSettings["database"];
            var collection = ConfigurationManager.AppSettings["collection"];
            var docDbclient = new DocumentDbClient(endpoint, authKey, database, collection);


            var tasks = Enumerable.Range(0, 10)
                .Select(i => GenerateRandomObject())
                .Select(r => Task.Run(async () => await docDbclient.CreateItemAsync(r))).ToList();

            Task.WaitAll(tasks.ToArray());
        }

        public static JObject GenerateRandomObject()
        {
            var eyeColors = new[] {"brown", "blue", "green"};
            var firstName = new[] {"Steve", "Joe", "Jeff", "Dan", "Steph", "Jen", "Hilary"};
            var lastName = new[] {"Smith", "Brown", "Peterman", "Clinton"};
            var gender = new[] {"male", "female", "other", "prefer not to specify"};
            var streets = new[] {"Fake St", "Memory Ln", "Microsoft Way"};

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
    }
}
