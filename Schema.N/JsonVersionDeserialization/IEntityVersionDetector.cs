using Newtonsoft.Json.Linq;

namespace Schema.N
{
    public interface IEntityVersionDetector
    {
        int GetEntityVersion(JObject json);
    }
}
