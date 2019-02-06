using System.Collections.Generic;
using Core.Models.Enums;
using Freecon.Models.ChatCommands;

namespace Server.Managers.ChatCommands
{
    public class TellCommand : BaseChatCommand, IChatCommand
    {
        protected PlayerManager _playerManager;

        public TellCommand(PlayerManager playerManager)
        {
            _playerManager = playerManager;

            CommandSignatures.AddRange(new [] { "tell", "t" });
        }

        public List<OutboundChatMessage> ParseChatline(ChatMetaData meta)
        {
            var split = ChatHelpers.GetSingleCommandArgument(meta.Arguments);

            if (split == null)
            {
                return meta.ReplyResponse(new ChatlineObject("Invalid tell command.", ChatlineColor.White));
            }

            var otherPlayer = _playerManager.GetPlayer(split.Item1);

            if (otherPlayer == null)
            {
                return meta.ReplyResponse(new ChatlineObject("Username " + split.Item1 + " not found.", ChatlineColor.White));
            }

            if (!otherPlayer.IsOnline)
            {
                return meta.ReplyResponse(new ChatlineObject("User " + split.Item1 + " is offline.", ChatlineColor.White));
            }

            var outboundChats = new List<OutboundChatMessage>();
            var commonText = new ChatlineFragment(split.Item2, ChatlineColor.White);

            var toOtherPlayer = new List<ChatlineFragment>()
            {
                new ChatlineFragment(meta.Player.Username + " tells you: ", ChatlineColor.Teal),
                commonText
            };

            var toSendingPlayer = new List<ChatlineFragment>()
            {
                new ChatlineFragment("You tell " + otherPlayer.Username + ": ", ChatlineColor.Teal),
                commonText
            };

            // Allows the client to keep track of who we should send replies to via /r
            var replyMetadata = new Dictionary<string, string>();
            replyMetadata["ReplyPlayer"] = meta.Player.Username;

            outboundChats.Add(new OutboundChatMessage(otherPlayer, new ChatlineObject(toOtherPlayer), replyMetadata));
            outboundChats.Add(new OutboundChatMessage(meta.Player, new ChatlineObject(toSendingPlayer)));

            return outboundChats;
        }
    }
}
