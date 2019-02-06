using System.Collections.Generic;
using Newtonsoft.Json;

namespace Freecon.Models.UI
{
    public class ClientChatLine
    {
        [JsonProperty("chatline")]
        public List<ClientChatFragment> Chatline { get; set; }

        [JsonProperty("timestamp")]
        public int Timestamp { get; set; }

        [JsonProperty("meta")]
        public Dictionary<string, string> Meta { get; set; } 
    }
}