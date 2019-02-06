using System.Collections.Generic;
using System.Linq;
using Freecon.Core;
using Freecon.Models.TypeEnums;
using Server.Database;
using Server.Managers;
using Server.Managers.Factories;
using Server.Models;
using SRServer.Services;

namespace DBFiller
{
    public class DBFillerUtils
    {
        private readonly IDatabaseManager _databaseManager;
        private readonly GalaxyManager _galaxyManager;
        private readonly LocatorService _locatorService;
        public readonly ShipFactory ShipFactory;
        public DBFillerUtils(IDatabaseManager databaseManager, GalaxyManager galaxyManager, LocatorService locatorService, WarpManager warpManager, LocalIDManager galaxyIdManager)
        {
            _databaseManager = databaseManager;
            _galaxyManager = galaxyManager;
            _locatorService = locatorService;
            ShipFactory = new ShipFactory(locatorService.RegistrationManager, warpManager, galaxyIdManager, locatorService, databaseManager);
        }

        public void CreateColonies(IEnumerable<Player> owningPlayers)
        {

            if (owningPlayers.Count() == 0)
            {
                return;
            }

            //PSystem sol = _galaxyManager.Systems.Find(ss => ss.Id == _dbFiller.gp.SolID);

            var areaList = _galaxyManager.GetAllAreas();

            string resultMessage = "";

            var itr = new CyclicalIterator<Player>(owningPlayers);
            itr.MoveNext();

            for (int i = 0; i < areaList.Count; i++)
            {
                if (areaList[i].AreaType != AreaTypes.Planet)
                    continue;

                var xPos = ((Planet)areaList[i]).Warpholes[0].PosX + 1;
                var yPos = ((Planet)areaList[i]).Warpholes[0].PosY + 1;

                var player = itr.Current;
                var ship = player.GetActiveShip();

                var success = _galaxyManager.TryColonizePlanet((Planet)areaList[i], ship, null, _locatorService, xPos, yPos,
                    out resultMessage, _databaseManager);

                itr.MoveNext();

            }

        }

        public void WarpPlayerToOwnedColony(Player player)
        {

            var areaList = _galaxyManager.GetAllAreas();

            var ownedColony =
                areaList.Where(c => c.AreaType == AreaTypes.Colony)
                    .Select(c => c as Colony)
                    .FirstOrDefault(c => c.OwnerID == player.Id);

            Helpers.DebugWarp(ownedColony, player, player.GetActiveShip());
        }
    }
}