using Core.Models.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Core.Web.Schemas
{
    public class LoginResponse
    {
        [JsonProperty(PropertyName = "result")]
        public LoginResult Result { get; set; }

        [JsonProperty(PropertyName = "hailMessage")]
        public HailMessages HailMessage { get { return HailMessages.ClientLogin; } }

        [JsonProperty(PropertyName = "serverIP")]
        //Address for client to connect to
        //public IPEndPoint ServerEndPoint { get; set; }//JSON can't handle IPEndPoint
        public byte[] ServerIP { get; set; }

        [JsonProperty(PropertyName = "serverPort")]
        public int ServerPort { get; set; }

        //Crypto
        [JsonProperty(PropertyName = "key")]
        public byte[] Key { get; set; }

        [JsonProperty(PropertyName = "iv")]
        public byte[] IV { get; set; }

        [JsonProperty(PropertyName = "apiToken")]
        public string ApiToken { get; set; }

        [JsonProperty(PropertyName = "currentAreaType")]
        public string CurrentAreaType { get; set; }
    }

}
