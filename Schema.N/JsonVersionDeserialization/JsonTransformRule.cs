using Newtonsoft.Json.Linq;
using System;

namespace Schema.N
{
    public class JsonTransformRule
    {
        public JsonTransformRuleType Operation { get; set; }
        public string TargetKey { get; set; }
        public object Value { get; set; }

        Action<JObject> _customMethod = null;
        public Action<JObject> CustomMethod {
            get
            {
                return _customMethod;
            }
            set
            {
                if(value != null)
                {
                    _customMethod = value;
                    Operation = JsonTransformRuleType.Custom;
                }
            }
        }

        public JsonTransformRule(string targetKey, JsonTransformRuleType operation, object value = null)
        {
            Operation = operation;
            TargetKey = targetKey;
            Value = value;
        }

        public JsonTransformRule(Action<JObject> custom)
        {
            CustomMethod = custom;
            Operation = JsonTransformRuleType.Custom;
            TargetKey = null;
            Value = null;
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
