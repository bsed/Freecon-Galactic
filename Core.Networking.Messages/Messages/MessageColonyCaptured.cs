namespace Freecon.Core.Networking.Models.Messages
{
    public class MessageColonyCaptured:MessagePackSerializableObject
    {
        public int OwnerTeamID { get { return _ownerTeamID; } set { _ownerTeamIDSet = true; _ownerTeamID = value; } }
        int _ownerTeamID;  
        bool _ownerTeamIDSet { get; set; }


        public override byte[] Serialize()
        {
            if (!_ownerTeamIDSet)
                throw new RequiredParameterNotInitialized("OwnerTeamID", this);

            return base.Serialize();
        }
    }
}
