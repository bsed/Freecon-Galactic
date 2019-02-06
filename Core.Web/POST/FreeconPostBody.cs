using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Core.Web.Post
{
    public class FreeconPostBody
    {
        [JsonProperty(PropertyName = "token")]
        public string Token { get; protected set; }
    }
}
