using Freecon.Core.Networking.Models;

namespace Server.Models.Interfaces
{
    public interface IOutgoingMessageService
    {
        void SendMessage(NetworkMessageContainer message);

    }
}
