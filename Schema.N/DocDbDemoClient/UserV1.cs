using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocDbDemoClient
{
    public class UserV1
    {
        public string id { get; set; }
        public string guid { get; set; }
        public string name { get; set; }
        public bool isActive { get; set; }
        public int age { get; set; }
        public string phone { get; set; }
        public string address { get; set; }
        public string about { get; set; }
        public string eyeColor { get; set; }
        public string gender { get; set; }
        public int SchemanVersion { get; set; }
    }
}
