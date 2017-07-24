using Newtonsoft.Json.Linq;

namespace JsonVersionDeserialization
{
    public interface IEntityVersionDetector
    {
        int GetEntityVersion(JObject json);
    }
}
