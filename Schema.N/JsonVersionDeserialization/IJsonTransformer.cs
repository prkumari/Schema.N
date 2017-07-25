using System.Collections.Generic;

namespace JsonVersionDeserialization
{
    interface IJsonTransformer
    {
        string ConvertTo(string from, string to, List<JsonTransformRule> rules=null);
    }
}
