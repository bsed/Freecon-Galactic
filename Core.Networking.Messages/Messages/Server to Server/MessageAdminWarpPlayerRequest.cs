using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Freecon.Core.Networking.Models.ServerToServer;

namespace Freecon.Core.Networking.ServerToServer
{
    public class MessageAdminWarpPlayerRequest : MessageServerToServer
    {
        public int ShipId { get; protected set; }

        public int CurrentAreaId { get; protected set; }

        public int NewAreaId { get; protected set; }

        public MessageAdminWarpPlayerRequest(int shipId, int currentAreaId, int newAreaId)
        {
            ShipId = shipId;
            CurrentAreaId = currentAreaId;
            NewAreaId = newAreaId;
        }
    }
}
