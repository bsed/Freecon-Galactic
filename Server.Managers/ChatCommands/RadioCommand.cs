using System.Collections.Generic;
using Core.Models.Enums;
using Freecon.Models.ChatCommands;

namespace Server.Managers.ChatCommands
{
    public class RadioCommand : BaseChatCommand, IChatCommand
    {

        public RadioCommand()
        {
            CommandSignatures.AddRange(new [] { "radio", "r" });
        }

        public List<OutboundChatMessage> ParseChatline(ChatMetaData meta)
        {
            var fragments = new List<ChatlineFragment>()
            {
                new ChatlineFragment(meta.Player.Username + " radios: ", ChatlineColor.Pink),
                new ChatlineFragment(meta.Arguments, ChatlineColor.White)
            };

            var chatline = new ChatlineObject(fragments);

            return meta.BuildReplyAllChatList(chatline, null);
        }
    }
}