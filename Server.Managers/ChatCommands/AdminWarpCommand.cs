using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.Models.Enums;
using Freecon.Core.Networking.Models;
using Freecon.Core.Networking.ServerToServer;
using Freecon.Models.ChatCommands;
using Freecon.Models.TypeEnums;
using RedisWrapper;
using Server.Database;
using Server.Models;
using Server.Models.Extensions;

namespace Server.Managers.ChatCommands
{
    public class AdminWarpCommand : BaseChatCommand, IAsyncChatCommand
    {
        private readonly IDatabaseManager _databaseManager;
        protected RedisServer _redisServer;
        private readonly Random _random;

        public AdminWarpCommand(IDatabaseManager databaseManager, RedisServer redisServer, Random random)
        {
            CommandSignatures.AddRange(new [] { "random-warp", "rw" });

            _databaseManager = databaseManager;
            _redisServer = redisServer;
            _random = random;
        }

        public async Task<List<OutboundChatMessage>> ParseChatline(ChatMetaData meta)
        {
            if (meta.Arguments?.Trim() == null)
            {
                return meta.ReplyResponse(new ChatlineObject("Invalid warp command.", ChatlineColor.White));
            }

            AreaTypes newAreaType;
            try
            {
                newAreaType = (AreaTypes)Enum.Parse(typeof(AreaTypes), meta.Arguments.Trim(), true);
            }
            catch (Exception e)
            {
                return meta.ReplyResponse(new ChatlineObject("Invalid warp destination type. Type \"system\". Error: " + e.Message, ChatlineColor.White));
            }
            
            var allAreasOfType = await _databaseManager.GetAllAreas(newAreaType);

            var allAreasOfTypeList = allAreasOfType.ToList();

            var newArea = allAreasOfTypeList[_random.Next(allAreasOfTypeList.Count)];

            if (newArea == null)
            {
                return meta.ReplyResponse(new ChatlineObject("Couldn't locate any areas of type.", ChatlineColor.White));
            }

            var msg = new NetworkMessageContainer();

            msg.MessageType = MessageTypes.Redis_AdminWarpPlayer;
            msg.MessageData = new MessageAdminWarpPlayerRequest(meta.Player.ActiveShipId.Value, meta.Player.CurrentAreaID.Value, newArea.Id);

            _redisServer.PublishObject(MessageTypes.Redis_AdminWarpPlayer, msg);

            return meta.ReplyResponse(new ChatlineObject("Warping you to " + newArea.AreaName + ".", ChatlineColor.White));
        }
    }
}
