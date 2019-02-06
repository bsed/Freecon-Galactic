namespace Freecon.Core.Networking.Models.Messages
{
    public class MessageChangeShipLocation:MessagePackSerializableObject
    {
        public int ShipID { get { return _shipID; } set { _shipIDSet = true; _shipID = value; } }
        int _shipID;
        bool _shipIDSet;

        public float PosX { get; set; }
        public float PosY { get; set; }
        public float Rotation { get; set; }
        public float VelX { get; set; }
        public float VelY { get; set; }

        public override byte[] Serialize()
        {
            if (!_shipIDSet)
                throw new RequiredParameterNotInitialized("ShipID", this);

            return base.Serialize();
        }

    }
}
