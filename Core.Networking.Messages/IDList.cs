using MsgPack.Serialization;
using System.Collections.Generic;
using System.IO;

namespace Freecon.Core.Networking.Models
{
    public class IDListData
    {
        public List<int> IDs { get; set; }

        public IDListData()
        {
            IDs = new List<int>();
        }

        public byte[] Serialize()
        {
            var serializer = MessagePackSerializer.Get<IDListData>();
            var stream = new MemoryStream();
            serializer.Pack(stream, this);
            return stream.GetBuffer();
        }

    }
}
