namespace Freecon.Core.Networking.Models.ServerToServer
{
    public class MessageSlaveConnectionRequest:MessagePackSerializableObject
    {
        /// <summary>
        /// ID generated locally on the slave
        /// </summary>
        public int SlaveID { get; set; }

    }

    public class MessageSlaveConnectionResponse : MessagePackSerializableObject
    {
        /// <summary>
        /// ID of the slave this message is targetting. If connection attempt is unsuccessful because of a collision, both slaves will get this message and retry connection.
        /// </summary>
        public int SlaveID { get; set; }

        public bool IsSuccessful { get; set; }
    }
}
