using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Freecon.Models.TypeEnums;
using Nancy;
using Server.Database;
using Server.Models;

namespace Core.Web.Modules
{
    public class GalaxyHandler : BaseHandler
    {
        private readonly IDatabaseManager _databaseManager;

        public GalaxyHandler(IDatabaseManager databaseManager) : base(RouteConfig.Galaxy)
        {
            _databaseManager = databaseManager;

            Get[RouteConfig.Galaxy_GetSystemInfo, runAsync:true] = _getSystemOverview;
        }

        public async Task<dynamic> _getSystemOverview(dynamic parameters, CancellationToken cancellationToken)
        {
            // Todo: Somehow fetch the current session to identify the user.

            int systemId = parameters.systemId;

            var area = await _databaseManager.GetAreaAsync(systemId, AreaTypes.System);

            if (area.AreaType != AreaTypes.System)
            {
                return SystemNotFound();
            }

            var system = area as PSystemModel;

            var rawPlanets = await _databaseManager.GetPlanetsAsync(system.PlanetIDs);

            var uncastedMoons = await _databaseManager.GetAreasAsync(system.MoonIDs);

            if (rawPlanets == null || uncastedMoons == null)
            {
                return InternalServiceError();
            }

            var rawMoons = uncastedMoons.Select(m => m as PlanetModel);

            // Convert planets into response format.
            var planets = rawPlanets.Select(p => new
            {
                PlanetType = p.PlanetType.ToString(),
                p.Id,
                p.Distance,
                p.CurrentTrip,
                p.MaxTrip,
                //Owned = p.OwnerID
            });

            // Why are moons separate?
            var moons = rawMoons.Select(p => new
            {
                PlanetType = p.PlanetType.ToString(),
                p.Id,
                p.IDToOrbit,
                p.Distance,
                p.CurrentTrip,
                p.MaxTrip,
                //Owned = p.OwnerID
            });

            // Flatten down properties for response.
            return ReturnJsonResponse(new
            {
                system.AreaName,
                moons,
                PlanetCount = system.PlanetIDs.Count,
                planets,
                system.SecurityLevel,
                system.Star,
                system.HasPort,
                system.Warpholes
            });
        }

        protected Response SystemNotFound()
        {
            return MessageWithStatus(HttpStatusCode.NotFound, "System Not Found");
        }
    }
}
