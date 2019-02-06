using Freecon.Core.Networking.Models.Objects;
using MsgPack.Serialization;

namespace Freecon.Core.Networking.Models.Messages
{
    public class MessageReceiveNewStructure:MessagePackSerializableObject
    {
        [MessagePackRuntimeType]
        public StructureData StructureData { get; set; }


    }
}
