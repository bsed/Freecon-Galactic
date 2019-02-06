using Freecon.Models.TypeEnums;
using SRServer.Services;

namespace Server.Models
{
    public class StarBase : Area<StarBaseModel>
    {
        protected StarBase()
        {
        }

        public StarBase(StarBaseModel s, LocatorService ls)
            : base(s, ls)
        {

        }


        public StarBase(int ID, LocatorService ls)
            : base(ID, ls)
        {
        }

        public override bool CanAddStructure(Player player, StructureTypes buildingType, float xPos, float yPos, out string resultMessage)
        {
            resultMessage = "You can't deploy a structure in a port, you'll kill somebody! Are you insane?";
            return false;
        }
    }

    public class StarBaseModel : AreaModel
    {
        public override AreaTypes AreaType { get { return AreaTypes.StarBase; } }
    }
}