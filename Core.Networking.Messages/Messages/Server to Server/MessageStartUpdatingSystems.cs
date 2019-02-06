using System.Collections.Generic;

namespace Freecon.Core.Networking.Models.ServerToServer
{
    public class MessageStartUpdatingSystems : MessageServerToServer
    {
        public List<int> IDsToSimulate { get; set; }

        public int SlaveServerID { get { return _slaveServerID; } set { _isSlaveServerIDSet = true; _slaveServerID = value; } }
        int _slaveServerID;
        bool _isSlaveServerIDSet;

        public bool ClearCurrentSystems { get; set; }

        public override byte[] Serialize()
        {
            if (!_isSlaveServerIDSet)
            {
                throw new RequiredParameterNotInitialized("SlaveServerID", this);
            }



            return base.Serialize();
        }

    }
}
