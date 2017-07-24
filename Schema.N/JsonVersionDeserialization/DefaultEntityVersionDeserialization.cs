using Newtonsoft.Json.Linq;

namespace JsonVersionDeserialization
{
    public class DefaultEntityVersionDeserialization<TEntity> : IEntityVersionDeserialization
    {
        public object Deserialize(JToken json)
        {
            return json.ToObject<TEntity>();
        }
    }
}
