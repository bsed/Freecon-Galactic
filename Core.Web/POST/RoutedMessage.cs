using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Freecon.Core.Networking.Models;
using Newtonsoft.Json;

namespace Core.Web.Post
{
    public class RoutedMessage:FreeconPostBody
    {
        [JsonProperty(PropertyName = "messageContainer")]
        public NetworkMessageContainer MessageContainer { get; set; }
    }
}
