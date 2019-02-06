namespace Freecon.Core.Networking.Models.Messages
{
    public class MessageStructureRequestResponse:MessagePackSerializableObject
    {
        public bool Approved { get; set; }

        public string Message {get; set;}

        public MessageStructureRequestResponse()
        {
            Message = "";
        }


    }
}
