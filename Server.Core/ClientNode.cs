namespace Freecon.Server.Core
{
    public interface IClientNode
    {
        ConnectionTypes ConnectionType { get; }
    }

    /// <summary>
    /// Keep this information around just in case any class needs to access 
    /// Something specific to the networking library's client connection.
    /// Please don't use this unless you really have to! Implement a generic
    /// Method inside of IClientNode. It makes life easier to port code.
    /// </summary>
    public enum ConnectionTypes
    {
        Lidgren,
        // Todo: ZeroMQ, Rest, etc
    }

    public interface IClientConnection
    {

    }
}
