using Freecon.Core.Networking.Models.Objects;
using System.Collections.Generic;

namespace Freecon.Core.Networking.Models.Messages
{
    public class MessagePositionUpdateData : MessagePackSerializableObject
    {
        public List<PositionUpdateData> UpdateDataObjects { get; set; }

        public int? SendingPlayerID
        {
            get { return _sendingPlayerID; }
            set
            {
                _sendingPlayerIDSet = value != null;
                _sendingPlayerID = value;
            }
        }
        protected int? _sendingPlayerID;
        protected bool _sendingPlayerIDSet;

        public int AreaID { get { return _areaID; } set { _areaID = value;
            _areaIDSet = true;
        } }

        protected int _areaID;
        protected bool _areaIDSet;

        public MessagePositionUpdateData()
        {
            UpdateDataObjects = new List<PositionUpdateData>();
        }

        public MessagePositionUpdateData(int? sendingPlayerID, int areaID) : this()
        {
            SendingPlayerID = sendingPlayerID;
            AreaID = areaID;
        }

        public override byte[] Serialize()
        {

            if (!_sendingPlayerIDSet)
            {
                throw new RequiredParameterNotInitialized("SendingPlayerID", this);
            }

            if (!_areaIDSet)
            {
                throw new RequiredParameterNotInitialized("AreaID", this);
            }

            return base.Serialize();
        }

    }
}
