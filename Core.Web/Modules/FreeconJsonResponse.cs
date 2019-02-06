using Newtonsoft.Json;

namespace Core.Web.Modules
{
    public class FreeconJsonResponse
    {
        [JsonProperty(propertyName: "message")]
        public string Message { get; protected set; }

        [JsonProperty(propertyName: "meta")]
        public object Meta { get; protected set; }

        public FreeconJsonResponse(string message, object meta)
        {
            Message = message;
            Meta = meta;
        }
    }
}