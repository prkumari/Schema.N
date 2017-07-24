using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace JsonVersionDeserialization
{
    public interface IEntityVersionDeserialization
    {
        object Deserialize(JToken json);
    }
}
