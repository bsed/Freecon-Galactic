using Freecon.Core.Networking.Models.Objects;
using MsgPack.Serialization;
using System.Collections.Generic;

namespace Freecon.Core.Networking.Models.Messages
{
    public class MessageAddCargoToShip:MessagePackSerializableObject
    {
        public int ShipID { get { return _shipID; } set { _isShipIDSet = true; _shipID = value; } }
        int _shipID;
        bool _isShipIDSet;


        [MessagePackRuntimeCollectionItemType]
        public List<StatefulCargoData> StatefulCargoData { get; set; }

        public List<StatelessCargoData> StatelessCargoData { get; set; }

        public MessageAddCargoToShip()
        {
            StatefulCargoData = new List<StatefulCargoData>();
            StatelessCargoData = new List<StatelessCargoData>();
        }

        public override byte[] Serialize()
        {
            if (!_isShipIDSet)
            {
                throw new RequiredParameterNotInitialized("ShipID", this);
            }

            return base.Serialize();
        }
    }

}
