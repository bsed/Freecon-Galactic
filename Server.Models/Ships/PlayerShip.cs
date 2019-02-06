using Core.Models;
using Freecon.Models.TypeEnums;
using Lidgren.Network;
using SRServer.Services;

namespace Server.Models
{
    public class PlayerShip : Ship<PlayerShipModel>
    {

        public PlayerShip() 
        {
            PilotType = PilotTypes.Player;
        }

        public PlayerShip(PlayerShipModel s, LocatorService ls)
            : base(s, ls)
        {


        }
        public PlayerShip(ShipStats shs, LocatorService ls)
            : base(shs, ls)
        {
            PilotType = PilotTypes.Player;
        }
           
    }
}