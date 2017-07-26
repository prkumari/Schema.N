using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Schema.N
{
    public class JsonTransformerVersionNextConverter<TPocoThis, TPocoNext> : AbstractVersionNextConverter<JObject, TPocoThis, TPocoNext>
    {
        /// <summary>
        /// Robot in Disguise.
        /// </summary>
        public IJsonTransformer Transformer { get; set; }

        public JsonTransformerVersionNextConverter(params JsonTransformRule[] rules)
        {
            Transformer = new JsonTransformer(rules);
        }

        public JsonTransformerVersionNextConverter(IJsonTransformer transformer)
        {
            Transformer = transformer;
        }

        protected override DataVersionInfo<JObject, TPocoNext> ConvertToNext(
            DataVersionInfo<JObject, TPocoThis> convertedPrev)
        {
            // Returning default TPocoNext as it is not required.
            // TODO but is it though, we should use the same version resolvers to do this i think.??
            return new DataVersionInfo<JObject, TPocoNext>(Transformer.ConvertTo(convertedPrev.RawData), default(TPocoNext));
        }
    }
}
