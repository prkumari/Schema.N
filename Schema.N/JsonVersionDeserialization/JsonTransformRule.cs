namespace Schema.N
{
    public class JsonTransformRule
    {
        public JsonTransformRuleType Operation { get; set; }
        public string TargetPath { get; set; }
        public object Value { get; set; }
    }

    public enum JsonTransformRuleType
    {
        Rename,
        Delete,
        CopyToken,
        NewProperty,
        SetValue,
        Custom
    }
}
