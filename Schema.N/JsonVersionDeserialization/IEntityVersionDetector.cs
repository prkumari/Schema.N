using Newtonsoft.Json.Linq;

namespace JsonVersionDeserialization
{
    public interface IEntityVersionDetector
    {
        string GetEntityVersion(JToken json);
    }
}
