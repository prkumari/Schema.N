using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Schema.N
{
    public class JsonTransformer : IJsonTransformer
    {
        public string ConvertTo(string from, string to, List<JsonTransformRule> rules=null)
        {
            // 'from' values take precedence if they exist in 'to'
            var jFrom = JObject.Parse(from);
            var jTo = JObject.Parse(to);

            jTo.Merge(jFrom, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Replace,
                MergeNullValueHandling = MergeNullValueHandling.Merge
            });

            if (rules != null)
            {
                foreach (var rule in rules)
                {
                    if (rule.Operation == JsonTransformRuleType.Rename)
                    {
                        HandleRename(rule.TargetPath, rule.Value.ToString(), jTo);
                    }
                    else if (rule.Operation == JsonTransformRuleType.Delete)
                    {
                        HandleDelete(rule.TargetPath, jTo);
                    }
                    else if (rule.Operation == JsonTransformRuleType.CopyToken)
                    {
                        HandleCopy(rule.TargetPath, rule.Value.ToString(), jTo);
                    }
                    else if (rule.Operation == JsonTransformRuleType.NewProperty)
                    {
                        HandleNewProperty(rule.Value.ToString(), jTo, rule.TargetPath);
                    }
                    else if (rule.Operation == JsonTransformRuleType.SetValue)
                    {
                        HandleSetValue(rule.TargetPath, rule.Value, jTo);
                    }
                }
            }

            return jTo.ToString();
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
            JToken targetToken;
            if(targetpath == null)
            {
                targetToken = model.Root;
            }
            else
            {
                targetToken = model.SelectToken(targetpath);
            }

            var newToken = new JProperty(propertyname, "");

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
                dynamic jObj = ObtainValue(value);
                if(jObj == null)
                {
                    throw new ArgumentException("Could not translate value!");
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
