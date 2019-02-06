using Freecon.Core.Networking.Models;
using Freecon.Core.Networking.Models.Proto;
using System;
using System.Collections.Generic;

namespace Freecon.Core.Networking.Proto
{
    public class MapperConfig
    {
        public Dictionary<Type, MessageTypes> TypeToMessageEnum = new Dictionary<Type, MessageTypes>()
        {
            { typeof(PositionUpdateProto), MessageTypes.PositionUpdateData }
        };
    }
}
