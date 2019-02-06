using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Freecon.Models.ChatCommands;
using Server.Models;
using Core.Models.Enums;
using Freecon.Models.TypeEnums;

namespace Server.Managers.ChatCommands
{
    //TODO: Consolidate this with ChatManager? What's the difference between the purposes of the two classes?
    public static class ChatHelpers
    {
        public static Regex SingleArgumentCommandRegex = new Regex(@"(\S*) (.*)");

        public static Tuple<string, string> GetSingleCommandArgument(string arguments)
        {
            var split = SingleArgumentCommandRegex.Match(arguments);

            if (split.Groups.Count == 3)
            {
                return new Tuple<string, string>(split.Groups[1].Value, split.Groups[2].Value);
            }

            return null;
        }

        public static List<OutboundChatMessage> ReplyResponse(this ChatMetaData meta, ChatlineObject chatLine)
        {
            return new List<OutboundChatMessage>()
            {
                new OutboundChatMessage(meta.Player, chatLine)
            };
        }

        public static List<OutboundChatMessage> BuildReplyAllChatList(this ChatMetaData meta, ChatlineObject chatLine, int? playerIDToIgnore)
        {
            return BuildReplyAllChatList(meta.Area, meta.Player.Id, chatLine);
        }

        /// <summary>
        /// Builds a list of OutboundChatMessage objects. Leave playerIDToIgnore null if the message should go to all players.
        /// </summary>
        /// <param name="area"></param>
        /// <param name="playerIDToIgnore"></param>
        /// <param name="chatLine"></param>
        /// <returns></returns>
        public static List<OutboundChatMessage> BuildReplyAllChatList(
            this IArea area,
            int? playerIDToIgnore,
            ChatlineObject chatLine)
        {
            var replies = new List<OutboundChatMessage>();

            replies.AddRange(
                area.GetOnlinePlayers()
                    .Select(kvp => kvp.Value)
                    .Where(p => p.Id != playerIDToIgnore && p.PlayerType == PlayerTypes.Human)
                    .Select(p => new OutboundChatMessage(p, chatLine))
            );
            
            return replies;
        }

        public static ChatlineColor GetColor(ChatTypes chatType)
        {
            switch (chatType)
            {
                case ChatTypes.Shout:
                    return ChatlineColor.Green;
                case ChatTypes.Alert:
                    return ChatlineColor.OrangeRed;
                case ChatTypes.Error:
                    return ChatlineColor.Red;
                case ChatTypes.Radio:
                    return ChatlineColor.Pink;
                case ChatTypes.Notification:
                    return ChatlineColor.Magenta;






                case ChatTypes.None:
                    return ChatlineColor.White;

                default:
                    {
                        ConsoleManager.WriteLine("Warning: GetColor not implemented for " + chatType.ToString());
                        return ChatlineColor.White;
                    }
            }
        }
        
    }
}