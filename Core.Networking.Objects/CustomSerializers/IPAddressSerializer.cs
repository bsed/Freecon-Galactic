using System.Net;
using MsgPack;
using MsgPack.Serialization;

namespace Freecon.Core.Networking.Objects.CustomSerializers
{
    public class IPAddressSerializer:MessagePackSerializer<IPAddress>
    {
        public IPAddressSerializer(SerializationContext context):base(context)
        {
           
        }

        protected override void PackToCore( Packer packer, IPAddress value )
        {

            packer.PackArrayHeader(1);
            packer.Pack(value.ToString());
           
        }

        protected override IPAddress UnpackFromCore(Unpacker unpacker)
        {
            string json;
            unpacker.ReadString(out json);
            return IPAddress.Parse(json);
        }

    }
}
