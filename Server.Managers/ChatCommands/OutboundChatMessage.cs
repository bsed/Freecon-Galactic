using System.Collections.Generic;
using Freecon.Models.ChatCommands;
using Server.Models;

namespace Server.Managers.ChatCommands
{
    public class OutboundChatMessage
    {
        public Player DestinationPlayer { get; protected set; }

        public ChatlineObject ChatMessage { get; protected set; }

        public Dictionary<string, string> Metadata { get; protected set; }

        public OutboundChatMessage(Player destination, ChatlineObject chat, Dictionary<string, string> meta)
            : this(destination, chat)
        {
            Metadata = meta;
        }

        public OutboundChatMessage(Player destination, ChatlineObject chat)
        {
            DestinationPlayer = destination;
            ChatMessage = chat;
        }
    }
}