using System.Collections.Generic;

namespace Schema.N
{
    interface IJsonTransformer
    {
        string ConvertTo(string from, string to, List<JsonTransformRule> rules=null);
    }
}
