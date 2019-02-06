namespace Freecon.Core.Networking.Models.Objects
{
    public class CommandCenterData:StructureData
    {
        public int? OwnerDefaultTeamID;

        public CommandCenterData()
        { }

        public CommandCenterData(StructureData d, int ownerDefaultTeamID):base(d)
        {
            OwnerDefaultTeamID = ownerDefaultTeamID;
        }
    }
}