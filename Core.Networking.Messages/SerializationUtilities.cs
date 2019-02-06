using System.IO;
using Freecon.Core.Networking.Models;
using Lidgren.Network;
using MsgPack.Serialization;
using System.Text;
using Freecon.Core.Networking.Objects.CustomSerializers;
using Freecon.Core.Utils;
using Newtonsoft.Json;

namespace Core.Utilities
{
    public class SerializationUtilities
    {
        static protected JsonSerializerSettings _defaultJsonSerializerSettings;

        static SerializationUtilities()
        {
            _defaultJsonSerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
            _defaultJsonSerializerSettings.Converters.Add(new IPAddressConverter());
            _defaultJsonSerializerSettings.Converters.Add(new IPEndPointConverter());
            SerializationContext.Default.Serializers.Register(new IPAddressSerializer(SerializationContext.Default));
        }


        static public T DeserializeMsgPack<T>(byte[] rawBytes)
         where T : MessagePackSerializableObject, new()
        {

#if DEBUG
            var data = JsonConvert.DeserializeObject<T>(Encoding.ASCII.GetString(rawBytes), _defaultJsonSerializerSettings);
#else
            var serializer = MessagePackSerializer.Get<T>();
            var stream = new MemoryStream(rawBytes);

            T data = serializer.Unpack(stream);
#endif   


            return data;
        }






        static public T DeserializeMsgPack<T>(NetIncomingMessage lidgrenMessage)
           where T : MessagePackSerializableObject, new()
        {
            var crc = lidgrenMessage.ReadUInt32();
            var size = lidgrenMessage.ReadInt32();

            // Data length is wrong.
            if (size != lidgrenMessage.LengthBytes - 8)
            {
                return null;
            }

            var rawBytes = lidgrenMessage.ReadBytes(size);

            // Data was corrupted.
            if (crc != Crc32.Compute(rawBytes))
            {
                return null;
            }

            return DeserializeMsgPack<T>(rawBytes);
        }

    }


}
