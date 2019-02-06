using Freecon.Core.Utils;
using Freecon.Models.ChatCommands;

namespace Server.Models.Extensions
{
    public static class ServerChatUtilities
    {
        public static string ToClientJson(this ChatlineObject chat)
        {
            var clientsideChat = new
            {
                chatline = chat.ChatFragments,
                timestamp = TimeKeeper.GetUnixTime()
            };

            return LowercaseContractResolver.SerializeObject(clientsideChat);
        }
    }
}
