using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Schema.N
{
    public interface IVersionMatcher
    {
        int EntityVersion { get; set; }

        int Weight { get; set; }

        Func<JObject, bool>  EntityMatchesFunc { get; set; }
    }
}
