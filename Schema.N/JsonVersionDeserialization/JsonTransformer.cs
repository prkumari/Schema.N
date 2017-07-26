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

        public string ConvertTo(string from, string to, JsonLoadSettings settings = null)
        {
            // 'from' values take precedence if they exist in 'to'
            var jFrom = JObject.Parse(from, settings);
            var jTo = JObject.Parse(to, settings);

            ConvertTo(jFrom, jTo);


            return jTo.ToString();
        }

        public void ConvertTo(JObject jFrom, JObject jTo)
        {
            jTo.Merge(jFrom, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Replace,
                MergeNullValueHandling = MergeNullValueHandling.Merge
            });

            if (TransformRules != null)
            {
                foreach (var rule in TransformRules)
                {
                    if (rule.Operation == JsonTransformRuleType.Rename)
                    {
                        HandleRename(rule.TargetKey, rule.Value.ToString(), jTo);
                    }
                    else if (rule.Operation == JsonTransformRuleType.Delete)
                    {
                        HandleDelete(rule.TargetKey, jTo);
                    }
                    else if (rule.Operation == JsonTransformRuleType.CopyToken)
                    {
                        HandleCopy(rule.TargetKey, rule.Value.ToString(), jTo);
                    }
                    else if (rule.Operation == JsonTransformRuleType.NewProperty)
                    {
                        HandleNewProperty(rule.TargetKey, jTo, rule.Value as string);
                    }
                    else if (rule.Operation == JsonTransformRuleType.SetValue)
                    {
                        HandleSetValue(rule.TargetKey, rule.Value, jTo);
                    }
                    else if (rule.Operation == JsonTransformRuleType.Custom)
                    {
                        HandleCustomInvoke(rule.CustomMethod, jTo);
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

        private void HandleCustomInvoke(Action<JObject> custom, JObject model)
        {
            if (custom == null)
                throw new ArgumentException("Custom Rule specified but action is null!");

            custom.Invoke(model);
        }

        private void HandleRename(string target, string value, JObject model)
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

        private void HandleDelete(string target, JObject model)
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

        private void HandleCopy(string target, string value, JObject model)
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

        private void HandleNewProperty(string propertyname, JObject model, string targetpath = null)
        {
            if(string.IsNullOrEmpty(propertyname))
            {
                throw new ArgumentException("Valid Property Name not provided!");
            }

            JToken targetToken;
            if(targetpath == null)
            {
                targetToken = model.Root;
            }
            else
            {
                targetToken = model.SelectToken(targetpath);
            }

            var newToken = new JProperty(propertyname, null);

            var parent = targetToken.Parent;
            if (parent == null) {
                model.Add(newToken);
            }   
            else
            {
                var container = targetToken as JObject;
                if (container != null)
                {
                    container.Add(newToken);
                }
            }
        }

        private void HandleSetValue(string target, object value, JObject model)
        {
            var token = model.SelectToken(target);

            if (token != null)
            {
                dynamic jObj = null;
                if (value != null)
                {
                    jObj = ObtainValue(value);
                    if (jObj == null)
                    {
                        throw new ArgumentException("Could not translate value!");
                    }
                }

                var parent = token.Parent;
                if (parent == null)
                    throw new InvalidOperationException("The parent is missing.");

                var parentProp = parent as JProperty;
                if (parentProp != null) {
                    var newToken = new JProperty(parentProp.Name, jObj);
                    parent.Replace(newToken);
                }
            }
        }

        private dynamic ObtainValue(object value)
        {
            dynamic result = null;
            try
            {
                result = JObject.FromObject(value);
                return result;
            }
            catch { }

            try
            {
                result = JObject.Parse(value.ToString());
                return result;
            }
            catch { }

            try
            {
                if (value.GetType() != typeof(string))
                {
                    result = value;
                }
                else
                {
                    result = value.ToString();
                }
                return result;
            }
            catch { }

            return result;
        }
    }
}
