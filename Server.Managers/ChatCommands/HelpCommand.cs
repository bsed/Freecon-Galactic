using System.Collections.Generic;
using Core.Models.Enums;
using Freecon.Models.ChatCommands;

namespace Server.Managers.ChatCommands
{
    public class HelpCommand : IChatCommand
    {
        protected List<string> _commandSignatures = new List<string>()
        {
            "help",
            "h"
        };

        public List<string> CommandSignatures { get { return _commandSignatures; } }

        public List<OutboundChatMessage> ParseChatline(ChatMetaData meta)
        {
            return meta.ReplyResponse(new ChatlineObject(ChatText.Help, ChatlineColor.White));
        }
    }
}
