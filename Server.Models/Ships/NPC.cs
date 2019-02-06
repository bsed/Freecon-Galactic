using Freecon.Models.TypeEnums;
using SRServer.Services;
using Server.Models.Interfaces;
using Core.Models;
using Server.Interfaces;

namespace Server.Models
{
    public class NPCShip : Ship<NPCShipModel>, ITeamable
    {
        public IShip currentTarget = null;

        protected NPCShip() 
        {
            PilotType = PilotTypes.NPC;
        }

        public NPCShip(ShipStats shs, LocatorService ls)
            : base(shs, ls)
        {
            PilotType = PilotTypes.NPC;
            IsNPC = true;
        }

        public NPCShip(NPCShipModel s, LocatorService ls)
            : base(s, ls)
        {

        }
        
    }
    
}