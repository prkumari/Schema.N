namespace Schema.N
{
    public class JsonTransformRule
    {
        public JsonTransformRuleType Operation { get; set; }
        public string TargetPath { get; set; }
        public object Value { get; set; }

        public JsonTransformRule(string targetPath, JsonTransformRuleType operation, object value = null)
        {
            Operation = operation;
            TargetPath = targetPath;
            Value = value;
        }
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
