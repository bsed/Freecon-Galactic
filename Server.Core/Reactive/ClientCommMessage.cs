using Freecon.Core.Networking.Models;

namespace Freecon.Server.Core.Reactive
{
    public class ClientCommMessage<T> where T : ICommMessage
    {
        public T Message { get; private set; }

        public IClientConnection Connection { get; private set; }

        public Player Player { get; private set; }

        public ClientCommMessage(T message, IClientConnection connection, Player player)
        {
            Message = message;

            Connection = connection;

            Player = player;
        }
    }
}
