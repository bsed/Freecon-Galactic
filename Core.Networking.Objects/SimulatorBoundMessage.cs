using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Freecon.Core.Networking.Models;

namespace Freecon.Core.Networking.Objects
{
    public class SimulatorBoundMessage:NetworkMessageContainer
    {
        /// <summary>
        /// Used to route the message to the appropriate simulator, and allows the simulator to route the message to the appropriate area
        /// </summary>
        public int TargetAreaId { get; set; }

        public SimulatorBoundMessage(MessagePackSerializableObject data, MessageTypes messageType, int targetAreaId) : base(data, messageType)
        {
            TargetAreaId = targetAreaId;
        }
    }
}
