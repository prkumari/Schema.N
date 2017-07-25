using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace JsonVersionDeserialization
{
    public class JsonConvertor : IJsonConvertor
    {
        public string ConvertTo(string from, string to, List<JsonConvertorRule> rules=null)
        {
            // 'from' values take precedence if they exist in 'to'
            var jFrom = JObject.Parse(from);
            var _jFrom = jFrom.DeepClone();
            var jTo = JObject.Parse(to);
            var _jTo = jTo.DeepClone();

            jTo.Merge(jFrom, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Union,
                MergeNullValueHandling = MergeNullValueHandling.Merge
            });

            if (rules != null)
            {
                foreach (var rule in rules)
                {
                    if (rule.Operation == JsonConvertorRuleType.Rename)
                    {
                        HandleRename(rule.TargetPath, rule.Value, jTo);
                    }
                }
            }

            return jTo.ToString();
        }

        public void HandleRename(string target, string value, JObject model)
        {
            var tokens = model.SelectTokens(target);
            foreach(var token in tokens)
            {
                var parent = token.Parent;
                if (parent == null)
                    throw new InvalidOperationException("The parent is missing.");
                var newToken = new JProperty(value, token);
                parent.Replace(newToken);
            }
        }
    }
}
