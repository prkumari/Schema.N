using System;
using Newtonsoft.Json.Linq;

namespace Schema.N
{
    public class DefaultEntityVersionDeserialization<TEntity> : IEntityVersionDeserialization
    {
        public object Deserialize(JObject json)
        {
            return json.ToObject<TEntity>();
        }

        public Type GetEntityType()
        {
            return typeof (TEntity);
        }
    }
}
