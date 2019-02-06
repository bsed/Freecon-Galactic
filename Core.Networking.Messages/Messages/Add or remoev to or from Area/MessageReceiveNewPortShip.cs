namespace Freecon.Core.Networking.Models.Messages
{
    public class MessageReceiveNewPortShip:MessagePackSerializableObject
    {
        /// <summary>
        /// For Simulator
        /// </summary>
        public int PortID{get; set;}

        public int Id { get; set; }

        public string Username { get; set; }



    }
}
