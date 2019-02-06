using System.Collections.Generic;
using Core.Models.Enums;
using Freecon.Core.Networking.Models;
using Freecon.Core.Networking.ServerToServer;
using Freecon.Models.ChatCommands;
using RedisWrapper;
using Server.Models.Extensions;

namespace Server.Managers.ChatCommands
{
    public class ShoutCommand : BaseChatCommand, IChatCommand
    {
        protected RedisServer _redisServer;

        public ShoutCommand(RedisServer redisServer)
        {
            CommandSignatures.AddRange(new [] { "shout", "s" });

            _redisServer = redisServer;
        }

        public List<OutboundChatMessage> ParseChatline(ChatMetaData meta)
        {
            if (meta.Arguments == null || meta.Arguments.Trim().Length == 0)
            {
                return new List<OutboundChatMessage>()
                {
                    new OutboundChatMessage(meta.Player, new ChatlineObject("Invalid shout command.", ChatlineColor.White))
                };
            }

            var actionText = meta.Player.Username + " " + ChatText.Shout.ToString() + ": ";

            var message = new ChatlineObject(new List<ChatlineFragment>()
            {
                new ChatlineFragment(actionText, ChatlineColor.Lime),
                new ChatlineFragment(meta.Arguments, ChatlineColor.White)
            });

            NetworkMessageContainer msg = new NetworkMessageContainer();
            msg.MessageData = new MessageRedisShout(meta.Player.Id, message.ToClientJson());
            msg.MessageType = MessageTypes.Redis_Shout;

            _redisServer.PublishObject(
                MessageTypes.Redis_Shout,
                msg
            );

            return null;
        }
    }
}
