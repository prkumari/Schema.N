using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonVersionDeserialization
{
    public class JsonConvertorRule
    {
        public JsonConvertorRuleType Operation { get; set; }
        public string TargetPath { get; set; }
        public string Value { get; set; }
    }

    public enum JsonConvertorRuleType
    {
        Rename
    }
}
