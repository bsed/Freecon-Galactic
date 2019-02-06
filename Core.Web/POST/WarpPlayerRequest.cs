using Newtonsoft.Json;

namespace Core.Web.Post
{
    public class WarpPlayerRequest : FreeconPostBody
    {
        [JsonProperty(PropertyName = "username")]
        public string Username { get; protected set; }
        
        [JsonProperty(PropertyName = "areaType")]
        public string AreaType { get; protected set; }

        [JsonProperty(PropertyName = "newAreaId")]
        public int? NewAreaId { get; protected set; }
    }
}