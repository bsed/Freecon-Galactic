using System.Linq;
using Freecon.Core;
using Freecon.Models.TypeEnums;
using Server.Managers;
using Server.Models;

namespace DBFiller
{
    /// <summary>
    /// Specifically made to fill a dbfiller object with state.
    /// This class is scanned with reflection, and the event handlers are registered in alphabetic order, and should fire in alphabetical order.
    /// Be sure to name all methods with an alphabetic prefix to preserve order as required. Method name must contain a '_' or it will not be registered.
    /// </summary>
    public class StateLoader
    {
        public void A_DistributePlayers(object sender, MinimalMockServer server)
        {
            var itr = new CyclicalIterator<PSystem>(server.GalaxyManager.Systems);

            if (server.HumanPlayers != null)
            {
                foreach (var p in server.HumanPlayers)
                {
                    if (p.Username == "ALLYOURBASE" || p.Username == "freeqaz")
                        continue;

                    var cs = itr.GetCurrentMoveNext();
                    Helpers.DebugWarp(cs, p, p.GetActiveShip());

                }
            }
        }

        public void B_PutPlayersInPort(object sender, MinimalMockServer server)
        {
            var ayb = server.PlayerManager.GetPlayer("ALLYOURBASE");
            var port = server.GalaxyManager.GetAllAreas().FirstOrDefault(area =>  area.AreaType == AreaTypes.Port);

            Helpers.DebugWarp(port, ayb, ayb.GetActiveShip());

            ConsoleManager.WriteLine(ayb.Username + " PlayerID: " + ayb.Id + " AreaID: " + ayb.CurrentAreaID + " ShipID: " + ayb.ActiveShipId, ConsoleMessageType.Debug);

        }
        
    }

    
}
