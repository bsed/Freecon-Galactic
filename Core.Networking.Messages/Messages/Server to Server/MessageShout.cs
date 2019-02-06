using Freecon.Core.Networking.Models.ServerToServer;

namespace Freecon.Core.Networking.ServerToServer
{
    public class MessageRedisShout : MessageServerToServer
    {
        public int PlayerID { get; set; }

        public string Message { get; set; }

        public MessageRedisShout()
        { }

        public MessageRedisShout(int playerID, string message) { PlayerID = playerID; Message = message; }

    }
}
