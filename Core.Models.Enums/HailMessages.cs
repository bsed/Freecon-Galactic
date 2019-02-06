namespace Core.Models.Enums
{
    /// <summary>
    /// Used to idenfity incoming connections
    /// </summary>
    public enum HailMessages : byte
    {
        MasterServer,
        SlaveServer,
        ClientLogin,//Used when the client attempts to connect to both master and slave during login
        ClientHandoff,//Used when the client is instructed to connect to a new slave after a handoff
    }
}
