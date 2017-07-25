using System.Collections.Generic;

namespace JsonVersionDeserialization
{
    interface IJsonConvertor
    {
        string ConvertTo(string from, string to, List<JsonConvertorRule> rules=null);
    }
}
