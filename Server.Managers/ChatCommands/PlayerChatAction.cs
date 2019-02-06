using Freecon.Models.ChatCommands;

namespace Server.Managers.ChatCommands
{
    public class PlayerChatAction
    {
        public ChatMetaData MetaData { get; protected set; }
        public ChatlineObject Chatline { get; protected set; }

        public PlayerChatAction(ChatMetaData metaData, ChatlineObject chatline)
        {
            MetaData = metaData;
            Chatline = chatline;
        }
    }
}