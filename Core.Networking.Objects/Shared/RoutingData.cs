using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Models.Enums;

namespace Freecon.Core.Networking.Objects
{
    /// <summary>
    /// Not required for client-server messages.
    /// </summary>
    public class RoutingData
    {
        public MessageRoutingIdTypes TargetIdType { get; set; }

        public int TargetId { get; set; }

    }
}
