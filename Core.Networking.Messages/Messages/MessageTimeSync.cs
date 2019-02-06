namespace Freecon.Core.Networking.Models.Messages
{
    public class MessageTimeSync:MessagePackSerializableObject
    {
        public double TimeMS { get; set; }

        public int PlayerID { get; set; }

        public MessageTimeSync()
        {
        }

        public MessageTimeSync(int playerID, double timeMS)
        {
            PlayerID = playerID;
            TimeMS = timeMS;
        }

    }
}
