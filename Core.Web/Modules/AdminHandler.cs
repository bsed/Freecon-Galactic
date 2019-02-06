using System;
using System.Threading;
using System.Threading.Tasks;
using Server.Database;
using System.Collections.Generic;
using System.Linq;
using Core.Models.Enums;
using Core.Web.Modules;
using Core.Web.Post;
using Core.Web.Schemas.Galaxy;
using Freecon.Core.Networking;
using Freecon.Core.Networking.Models;
using Freecon.Core.Networking.ServerToServer;
using Freecon.Models.TypeEnums;
using RedisWrapper;
using Freecon.Server.Core;
using Nancy;
using Nancy.Security;
using Newtonsoft.Json.Serialization;
using Server.Models;

namespace Core.Web.NancyModules
{
    public class AdminHandler : BaseHandler
    {
        IDatabaseManager _databaseManager;
        RedisServer _redisServer;
        Random _random;

        public AdminHandler(IDatabaseManager databaseManager, RedisServer redisServer)
            : base(RouteConfig.Admin)
        {   
            _databaseManager = databaseManager;
            _redisServer = redisServer;
            _random = new Random();

            this.RequiresAuthentication();
            this.RequiresClaims(new List<string>() {UserRoles.Admin.ToString()});
            
            PostWithAdminAuth<WarpPlayerRequest>(RouteConfig.Admin_WarpPlayer, _warpPlayer);
            PostWithAdminAuth<WarpPlayerRequest>(RouteConfig.Admin_WarpPlayerToType, _warpPlayerToType);
            PostWithAdminAuth<RoutedMessage>(RouteConfig.Admin_DirectRoute, _routeMessage);
            GetWithAdminAuth(RouteConfig.Admin_GetMockGalaxy, _getMockGalaxy);

            Get[RouteConfig.Admin_GetAllSystems, runAsync: true] = _getLandData;
            
        }

        async Task<dynamic> _getMockGalaxy(dynamic parameters, PlayerSession session, CancellationToken cancellationToken)
        {
            var mockPortAttributes = new List<WebMetaAttribute>()
            {
                new WebMetaAttribute("MostCommonEthnicity", "Irish", WebMetaAttibuteType.str),
                new WebMetaAttribute("SomeGenerationWeight", "Potatos are plentiful here!", WebMetaAttibuteType.str)
            };

            var mockPort = new WebPort(mockPortAttributes, "Starbase", 10000, 12000, 1, 2, 3);

            var mockPlanetAttributes = new List<WebMetaAttribute>()
            {
                new WebMetaAttribute("UraniumPercentage", "0.5", WebMetaAttibuteType.float32),
                new WebMetaAttribute("BestYieldingCrop", "potato", WebMetaAttibuteType.str)
            };

            var mockPlanet = new WebPlanet(mockPlanetAttributes, "Earthlike", 9000, 11000, 2, null, 3);

            var mockSystemAttributes = new List<WebMetaAttribute>()
            {
                new WebMetaAttribute("SupernovaLikelihood", "0.6", WebMetaAttibuteType.float32),
                new WebMetaAttribute("PotatoFarmerHeaven", "true", WebMetaAttibuteType.str),
                new WebMetaAttribute("NumberOfJamisonsAllowed", "0", WebMetaAttibuteType.int32)
            };

            var systemPlanets = new List<WebPlanet>()
            {
                mockPlanet
            };

            var systemPorts = new List<WebPort>()
            {
                mockPort
            };

            var mockSystem = new WebSystem(systemPlanets, systemPorts, mockSystemAttributes, 3);

            var galaxySystems = new List<WebSystem>()
            {
                mockSystem
            };

            var mockGalaxyAttributes = new List<WebMetaAttribute>()
            {
                new WebMetaAttribute("NumberOfDicks", "9001", WebMetaAttibuteType.int32)
            };

            var mockGalaxy = new WebGalaxy(galaxySystems, mockGalaxyAttributes);

            return ReturnJsonResponse(mockGalaxy);
        }

        /// <summary>
        /// Forwards a message to the appropriate slave
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="session"></param>
        /// <param name="messageToRoute"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task<dynamic> _routeMessage(dynamic parameters, PlayerSession session, RoutedMessage messageToRoute, CancellationToken cancellationToken)
        {
            var routingType = messageToRoute.MessageContainer.RoutingData.TargetIdType;
            var routingId = messageToRoute.MessageContainer.RoutingData.TargetId;
            var msg = messageToRoute.MessageContainer;

            int destinationChannelId = 0;

            switch (routingType)
            {
                case MessageRoutingIdTypes.AreaId:
                {
                    var areaModel = await _databaseManager.GetAreaAsync(routingId);
                    if (areaModel == null)
                    {
                        return "AreaId not found while attempting admin message route.";
                    }

                    break;
                }

                case MessageRoutingIdTypes.PlayerId:
                {
                    var playerModel = await _databaseManager.GetPlayerAsync(routingId);
                    if (playerModel == null)
                    {
                        return "PlayerId not found while attempting admin message route.";
                    }
                    else if (playerModel.CurrentAreaID == null)
                    {
                        return "Player.CurrentAreaID was null when attempting admin message route.";
                    }

                    destinationChannelId = playerModel.CurrentAreaID.Value;

                    break;
                }
                case MessageRoutingIdTypes.ShipId:
                {
                    var shipModel = await _databaseManager.GetShipAsync(routingId);

                    if (shipModel == null)
                    {
                        return "ShipId not found while attempting admin message route.";
                    }
                    else if (shipModel.CurrentAreaID == null)
                    {
                        return "Ship.CurrentAreaID was null when attempting admin message route.";
                    }
                    destinationChannelId = shipModel.CurrentAreaID.Value;

                    break;
                }

            }

            _redisServer.PublishObjectAsync(ChannelTypes.WebToSlave, destinationChannelId, msg);
            return "Success";

        }

        /// <summary>
        /// Allows an admin to warp a player.
        /// Includes lots of error handling.
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="session"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task<dynamic> _warpPlayer(dynamic parameters, PlayerSession session, WarpPlayerRequest warpRequest, CancellationToken cancellationToken)
        {
            if (warpRequest.Username == null)
            {
                return MessageWithStatus(HttpStatusCode.BadRequest, "username was null");
            }

            if (warpRequest.NewAreaId == null)
            {
                return MessageWithStatus(HttpStatusCode.BadRequest, "newAreaId was null");
            }

            var newArea = await _databaseManager.GetAreaAsync(warpRequest.NewAreaId.Value);

            if (newArea == null)
            {
                return MessageWithStatus(HttpStatusCode.NotFound, "Unknown area requested");
            }

            var player = await _databaseManager.GetPlayerAsync(warpRequest.Username);

            if (player == null)
            {
                return MessageWithStatus(HttpStatusCode.NotFound, "Player not found");
            }

            if (!player.ActiveShipId.HasValue)
            {
                return MessageWithStatus(HttpStatusCode.BadRequest, "Player has no current ship");
            }

            if (!player.CurrentAreaID.HasValue)
            {
                return MessageWithStatus(HttpStatusCode.BadRequest, "Player has no current area");
            }

            WarpPlayerToArea(player, warpRequest.NewAreaId.Value);

            return MessageWithStatus(HttpStatusCode.OK, "Player online and warped successfully");
        }

        async Task<dynamic> _warpPlayerToType(dynamic parameters, PlayerSession session, WarpPlayerRequest warpRequest, CancellationToken cancellationToken)
        {
            if (warpRequest.Username == null || warpRequest.AreaType == null)
            {
                return MessageWithStatus(HttpStatusCode.BadRequest, "Invalid request params");
            }
            
            AreaTypes areaType;
            switch (warpRequest.AreaType.ToLower())
            {
                case "port":
                    areaType = AreaTypes.Port;
                    break;
                case "colony":
                    areaType = AreaTypes.Colony;
                    break;
                case "planet":
                    areaType = AreaTypes.Planet;
                    break;
                case "starbase":
                    areaType = AreaTypes.StarBase;
                    break;
                case "system":
                default:
                    areaType = AreaTypes.System;
                    break;
            }

            var playerTask = _databaseManager.GetPlayerAsync(warpRequest.Username);
            var allAreasOfType = await _databaseManager.GetAllAreas(areaType);

            var allAreasOfTypeList = allAreasOfType.ToList();

            var newArea = allAreasOfTypeList[_random.Next(allAreasOfTypeList.Count)];

            if (newArea == null)
            {
                return MessageWithStatus(HttpStatusCode.InternalServerError, "Couldn't locate any areas of type.");
            }

            var player = await playerTask;

            if (player == null)
            {
                return MessageWithStatus(HttpStatusCode.NotFound, "Couldn't locate player");
            }

            WarpPlayerToArea(player, newArea.Id);

            return MessageWithStatus(HttpStatusCode.OK, "Warped player from " + player.CurrentAreaID + " to " + newArea.AreaName);
        }

        async Task<dynamic> _getLandData(dynamic parameters, CancellationToken cancellationToken)
        {
            return 501; // Not implemented
        }

        private void WarpPlayerToArea(PlayerModel player, int newAreaId)
        {
            var msg = new NetworkMessageContainer();

            msg.MessageType = MessageTypes.Redis_AdminWarpPlayer;
            msg.MessageData = new MessageAdminWarpPlayerRequest(player.ActiveShipId.Value, player.CurrentAreaID.Value, newAreaId);
            
            _redisServer.PublishObject(MessageTypes.Redis_AdminWarpPlayer, msg);
        }
    }
}
