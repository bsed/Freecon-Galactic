using Lidgren.Network;

namespace Server.Models
{
    public interface IMessageManager
    {
        NetPeer Server { get; }

    }
}
