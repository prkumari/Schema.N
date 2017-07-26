using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocDbDemoClient
{
    class UserV2
    {
        public string id { get; set; }
        public string guid { get; set; }
        public string name { get; set; }
        public bool isActive { get; set; }
        public bool isActiveEmployee { get; set; }
        public int age { get; set; }
        public string phone { get; set; }
        public string address { get; set; }
        public string description { get; set; }
        public string gender { get; set; }
        public string company { get; set; }
        public string email { get; set; }
        public int SchemanVersion { get; set; }
    }
}
