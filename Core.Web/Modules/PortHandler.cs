using Nancy;
using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Web.Modules;
using Core.Web.Schemas.Port;
using Freecon.Core.Networking;
using Freecon.Models.TypeEnums;
using Newtonsoft.Json;
using Server.Database;
using RedisWrapper;
using Server.Models;

namespace Core.Web.NancyModules
{
    public class PortHandler : BaseHandler
    {
        IDatabaseManager _databaseManager;
        RedisServer _redisServer;

        public PortHandler(IDatabaseManager databaseManager, RedisServer redisServer)
            : base(RouteConfig.Port)
        {   //Method:GET
            //Pattern:land-data
            //Action: _getState
            GetWithPlayerAuth(RouteConfig.Port_GetState, _getState);
            GetWithPlayerAuth(RouteConfig.Port_GetPlayersInPort, _getPlayersInPort);

            _databaseManager = databaseManager;
            _redisServer = redisServer;

        }

        async Task<dynamic> _getState(dynamic parameters, PlayerSession session, CancellationToken cancellationToken)
        {
            var p = await _databaseManager.GetPlayerAsync(session.PlayerId);

            if (p?.ActiveShipId == null || !p.CurrentAreaID.HasValue)
            {
                return MessageWithStatus(HttpStatusCode.NotFound, "Player not found.");
            }

            var shipId = p.ActiveShipId.Value;
            var portAreaId = p.CurrentAreaID.Value;

            // I'm not sure if we need to await actions in nancy,
            // I'm pretty sure they all run fully concurrently anyway.
            var port = await _databaseManager.GetAreaAsync(portAreaId) as PortModel;

            if (port == null || !port.ShipIDs.Contains(shipId))
            {
                Console.WriteLine("Port not found or ship was not in port during port state request.");
                return MessageWithStatus(HttpStatusCode.NotFound, "Player not found.");
            }
            
            return ReturnJsonResponse(new PortStateDataResponse(port));
        }

        async Task<dynamic> _getPlayersInPort(dynamic parameters, PlayerSession session, CancellationToken cancellationToken)
        {

            return 501; // Not implemented
        }
    }
}
