using Freecon.Core.Networking.Models.Objects;
using System.Collections.Generic;

namespace Freecon.Core.Networking.Models.Messages
{
    public class MessageReceiveFloatyAreaObjects:MessagePackSerializableObject
    {
        public List<FloatyAreaObjectData> FloatyObjects;



        public MessageReceiveFloatyAreaObjects()
        {
            FloatyObjects = new List<FloatyAreaObjectData>();
        }

    }
}
