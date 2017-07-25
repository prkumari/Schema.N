using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Schema.N
{
    public interface IEntityVersionDeserialization
    {
        object Deserialize(JObject json);

        Type GetEntityType();
    }
}
