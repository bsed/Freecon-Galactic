using Freecon.Core.Networking.Models;

namespace Server.Models
{
    public class ServerNetworkMessage
    {
        public NetworkMessageContainer Message { get; protected set; }

        public Player SendingPlayer { get; protected set; }

        public ServerNetworkMessage(NetworkMessageContainer message, Player sendingPlayer)
        {
            Message = message;
            SendingPlayer = sendingPlayer;
        }
    }
}