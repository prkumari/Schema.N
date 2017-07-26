using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Schema.N
{
    public interface IJsonTransformer
    {
        string ConvertTo(string from, string to);

        void ConvertTo(JObject from, JObject to);

        JObject ConvertTo(JObject from);


    }
}
