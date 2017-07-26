using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Schema.N
{
    public class JsonTransformer : IJsonTransformer
    {
        public IReadOnlyList<JsonTransformRule> TransformRules => TransformRulesInternal;
        private List<JsonTransformRule> TransformRulesInternal { get; }

        public JsonTransformer(params JsonTransformRule[] rules) : this(rules.ToList())
        {
            
        }

        public JsonTransformer(List<JsonTransformRule> rules)
        {
            TransformRulesInternal = rules;
        }

        public string ConvertTo(string from, string to)
        {
            // 'from' values take precedence if they exist in 'to'
            var jFrom = JObject.Parse(from);
            var jTo = JObject.Parse(to);

            ConvertTo(jFrom, jTo);


            return jTo.ToString();
        }

        public void ConvertTo(JObject jFrom, JObject jTo)
        {
            jTo.Merge(jFrom, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Union,
                MergeNullValueHandling = MergeNullValueHandling.Merge
            });

            if (TransformRules != null)
            {
                foreach (var rule in TransformRules)
                {
                    if (rule.Operation == JsonTransformRuleType.Rename)
                    {
                        HandleRename(rule.TargetPath, rule.Value, jTo);
                    }
                    else if (rule.Operation == JsonTransformRuleType.Delete)
                    {
                        HandleDelete(rule.TargetPath, jTo);
                    }
                    else if (rule.Operation == JsonTransformRuleType.CopyToken)
                    {
                        HandleCopy(rule.TargetPath, rule.Value, jTo);
                    }
                }
            }
        }

        public JObject ConvertTo(JObject from)
        {
            var jto = new JObject();
            ConvertTo(from, jto);
            return jto;
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

        public void HandleDelete(string target, JObject model)
        {
            var tokens = model.SelectTokens(target);
            foreach (var token in tokens)
            {
                var parent = token.Parent;
                if (parent == null)
                    throw new InvalidOperationException("The parent is missing.");
                parent.Remove();
            }
        }

        public void HandleCopy(string target, string value, JObject model)
        {
            var copyFrom = model.SelectToken(target);
            var copyTo = model.SelectToken(value);
            if (copyTo == null)
            {
                throw new ArgumentException("Copy to node doesn't exist.");
            }
            if (copyFrom == null)
            {
                throw new ArgumentException("Copy from node doesn't exist.");
            }

            var copyToParent = copyTo.Parent;
            if (copyToParent == null)
                throw new InvalidOperationException("The parent is missing.");

            copyTo.Replace(copyFrom);
        }
    }
}
