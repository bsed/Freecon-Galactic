namespace Freecon.Core.Networking.Models.ServerToServer
{
    public class MessageSlavePing:MessagePackSerializableObject
    {
        public int SlaveID { get; set; }

    }
}
