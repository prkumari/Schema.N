namespace JsonVersionDeserialization
{
    public class JsonTransformRule
    {
        public JsonConvertorRuleType Operation { get; set; }
        public string TargetPath { get; set; }
        public string Value { get; set; }
    }

    public enum JsonConvertorRuleType
    {
        Rename,
        Delete,
        CopyToken,
        Replace
    }
}
