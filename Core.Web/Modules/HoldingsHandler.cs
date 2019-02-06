using Nancy;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Server.Database;
using Server.Models;
using Core.Web.Get;
using AutoMapper;
using Core.Web.Modules;
using Server.Models.Database;
using Freecon.Core.Networking;
using Server.Models.Database;

namespace Core.Web.NancyModules
{
    public class HoldingsHandler : BaseHandler
    {
        IDatabaseManager _databaseManager;

        public HoldingsHandler(IDatabaseManager databaseManager):base(RouteConfig.Holdings)
        {
            _databaseManager = databaseManager;
                      
            GetWithPlayerAuth(RouteConfig.Holdings_FetchState, _getHoldingsData);
        }
        
        async Task<dynamic> _getHoldingsData(dynamic parameters, PlayerSession session, CancellationToken cancellationToken)
        {
            var p = await _databaseManager.GetPlayerAsync(session.PlayerId);

            if (p == null)
            {
                return MessageWithStatus(HttpStatusCode.NotFound, "Player not found.");
            }

            //TODO: Implement redis cache
            var colonies = new List<Colony_HoldingsVM>();
            var planets = await _databaseManager.GetPlanetsAsync(p.ColonizedPlanetIDs) ?? new List<PlanetModel>();

            var colonyIDsToLoad = planets.Where(q => q.ColonyID.HasValue).Select(pl => pl.ColonyID.Value);

            var loadedcols = await _databaseManager.GetAreasAsync(colonyIDsToLoad) ?? new List<AreaModel>();
            foreach (AreaModel c in loadedcols)
            {
                colonies.Add(Mapper.Map<ColonyModel, Colony_HoldingsVM>((ColonyModel)c));
            }
            
            var response = new Client_HoldingsDataResponse();
            response.PlayerID = session.PlayerId;
            response.Colonies = colonies;

            return ReturnJsonResponse(response);
        }

    }
}
