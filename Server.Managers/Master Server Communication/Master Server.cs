using Core.Models.Enums;

namespace Server.Managers
{
    //public class MasterServer : INetworkIDSupplier
    //{
    //    ConnectionManager _connectionManager;

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <param name="myServer">Lidgren object representing this local slave</param>
    //    /// <param name="masterServerConnection"></param>
    //    public MasterServer(ConnectionManager cm)
    //    {            
    //        _connectionManager = cm;
    //    }      

    //    /// <summary>
    //    /// Sends a request to the master server for numToRequest unused GalaxyIDs
    //    /// </summary>
    //    /// <param name="numToRequest"></param>
    //    public void RequestFreeIDs(int numToRequest, IDTypes IDType)
    //    {
    //        NetOutgoingMessage msg = _connectionManager.Server.CreateMessage();
    //        msg.Write((byte)MessageTypes.MasterServerMessage);
    //        msg.Write((byte)MasterSlaveMessageTypes.GlobalIDRequest);
    //        msg.Write((byte)IDType);
    //        msg.Write(numToRequest);
    //        _connectionManager.Server.SendMessage(msg, _connectionManager.MasterServerConnection, NetDeliveryMethod.ReliableUnordered);
    //    }        

    //}    

    public interface INetworkIDSupplier
    {
        void RequestFreeIDs(int numToRequest, IDTypes IDType);
       
    }

        
}
