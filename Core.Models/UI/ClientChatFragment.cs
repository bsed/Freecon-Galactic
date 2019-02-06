using Newtonsoft.Json;

namespace Freecon.Models.UI
{
    public class ClientChatFragment
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }
    }
}