using Freecon.Core.Networking.Models.Objects;
using System.Collections.Generic;

namespace Freecon.Core.Networking.Models.Messages
{
    public class MessageRemoveCargoFromShip:MessagePackSerializableObject
    {

        public int ShipID { get { return _shipID; } set { _isShipIDSet = true; _shipID = value; } }
        int _shipID;
        bool _isShipIDSet;


        public List<int> StatefulCargoIDs { get; set; }

        public List<StatelessCargoData> StatelessCargoData { get; set; }

        public MessageRemoveCargoFromShip()
        {
            StatefulCargoIDs = new List<int>();
            StatelessCargoData = new List<StatelessCargoData>();
        }

        public override byte[] Serialize()
        {

            if(!_isShipIDSet)
            {
                throw new RequiredParameterNotInitialized("ShipID", this);
            }


            return base.Serialize();
        }
    }
}
