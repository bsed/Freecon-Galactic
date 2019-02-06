using Lidgren.Network;

namespace Freecon.Server.Core.Lidgren
{
    /// <summary>
    /// Exposes Lidgren network stats behind an interface.
    /// </summary>
    public class LidgrenClientNode : IClientNode
    {
        public NetConnection Connection { get; private set; }

        public ConnectionTypes ConnectionType { get { return ConnectionTypes.Lidgren; } }

        public LidgrenClientNode(NetConnection connection)
        {
            Connection = connection;
        }
    }
}
