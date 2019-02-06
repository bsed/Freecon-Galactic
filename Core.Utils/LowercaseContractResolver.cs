using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Freecon.Core.Utils
{
    public class LowercaseContractResolver : DefaultContractResolver
    {

        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            ContractResolver = new LowercaseContractResolver()
        };

        public static string SerializeObject(object o, Formatting formatting = Formatting.None)
        {
            return JsonConvert.SerializeObject(o, formatting, Settings);
        }

        protected override string ResolvePropertyName(string key)
        {
            return Char.ToLowerInvariant(key[0]) + key.Substring(1);
        }
    }
}
