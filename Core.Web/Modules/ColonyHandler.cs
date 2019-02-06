using System;
using Nancy;
using System.Threading;
using System.Threading.Tasks;
using Nancy.ModelBinding;
using Newtonsoft.Json;
using Server.Database;
using Freecon.Models.TypeEnums;
using Server.Models;
using System.Collections.Generic;
using Core.Web.Modules;
using Server.Models.Structures;
using Core.Web.Schemas;
using Freecon.Core.Networking;
using RedisWrapper;
using Freecon.Core.Networking.ServerToServer;
using Freecon.Core.Networking.Models;
using Freecon.Core.Utils;

namespace Core.Web.NancyModules
{
    public class ColonyHandler : BaseHandler
    {
        IDatabaseManager _databaseManager;
        RedisServer _redisServer;
        private readonly ILoggerUtil _logger;

        public ColonyHandler(IDatabaseManager databaseManager, RedisServer redisServer, ILoggerUtil logger)
            : base(RouteConfig.Colony)
        {   
            _databaseManager = databaseManager;
            _redisServer = redisServer;
            _logger = logger;

            Get[RouteConfig.Colony_GetLandData, runAsync: true] = _getLandData;

            GetWithPlayerAuth(RouteConfig.Colony_GetColonyData, _getColonyData);

            //Post[RouteConfig.Colony_PushState, runAsync: true] = _getColonyData;
        }

        async Task<dynamic> _getLandData(dynamic parameters, CancellationToken cancellationToken)
        {
            return 501; // Not implemented
        }

        async Task<dynamic> _getColonyData(dynamic parameters, PlayerSession session, CancellationToken cancellationToken)
        {
            var player = await _databaseManager.GetPlayerAsync(session.PlayerId);

            if (!player.CurrentAreaID.HasValue)
            {
                return 404;
            }

            var currentArea = await GetAreaFromDatabase(player.CurrentAreaID.Value);

            if (currentArea.AreaType != AreaTypes.Colony)
            {
                return 404;
            }

            var cm = currentArea as ColonyModel;
            
            if (cm == null)
            {
                return 404;
            }

            if (!cm.ParentAreaID.HasValue)
            {
                return InternalServiceError();
            }

            var pm = await GetAreaFromDatabase<PlanetModel>(cm.ParentAreaID.Value, AreaTypes.Planet);

            if (pm == null)
            {
                return 404;
            }

            var structureModels = await _databaseManager.GetStructuresAsync(pm.StructureIDs);

            if (structureModels == null)
            {
                return 404;
            }
            
            return ReturnJsonResponse(new ClientFullColonyStateDataResponse(cm, pm.PlanetType, structureModels));
        }

        async Task<dynamic> _pushColonyData(dynamic parameters, CancellationToken cancellationToken)
        {
            var mockResponses = new MockResponses();

            // Not sure if this works if there are unassigned properties
            var pushColonyData = this.BindAndValidate<MessageColonyDataPush>();

            Console.WriteLine("Received Push Data Request ${0}", pushColonyData);

            var fakeResponse = mockResponses.GetFakeColonyData();

            var msg = new NetworkMessageContainer();
            msg.MessageData = pushColonyData;

            // Slave gets data here
            _redisServer.PublishObject(MessageTypes.Redis_ColonyDataPush, msg);


            // Dunno what you plan to do here Free, but the server will have received the push by this point

            //fakeResponse.Pages.Overview.Sliders = pushColonyData.Sliders;

            return ReturnJsonResponse(fakeResponse);
        }

        /// <summary>
        /// Fetch area with known type from database.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="areaId"></param>
        /// <param name="areaType"></param>
        /// <returns></returns>
        public async Task<T> GetAreaFromDatabase<T>(int areaId, AreaTypes areaType) where T : AreaModel
        {
            try
            {
                return await _databaseManager.GetAreaAsync(areaId, areaType) as T;
            }
            catch (WrongAreaTypeException e)
            {
                _logger.Log("User requested area of different type. Potential race condition or hacking?", LogLevel.Warning);
            }

            return null;
        }

        /// <summary>
        /// Fetch area of unknown type from database.
        /// </summary>
        /// <param name="areaId"></param>
        /// <returns></returns>
        public async Task<AreaModel> GetAreaFromDatabase(int areaId)
        {
            return await _databaseManager.GetAreaAsync(areaId);
        }
    }

}
