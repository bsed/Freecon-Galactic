//namespace MessageEnums
//{
//    public enum MasterSlaveMessageTypes : byte
//    {
//        //Commands slave to begin or cease updating a set of system IDs
//        StartUpdatingSystems,
//        StopUpdatingSystems,

//        //Sending/receiving IDs
//        GlobalIDRequest,
//        GlobalIDResponse,

//        //Client Logins
//        LoginRequest,//Incoming client request
//        LoginResponse,//Master server response, slave IP sent here
//        HandleClient,//Tells the slave server to accept a client login
//        ClientLogout,

//        SlaveConnectRequest,//Slaves connecting to the master server
//        COMMANDConnect,//Orders a client to connect to a particular slave IP

//        ClientHandoff,//Used to signal a slave that a client ship is warping between servers

//        NullType
//    }

//    /// <summary>
//    /// Used to idenfity incoming connections
//    /// </summary>
//    public enum HailMessages : byte
//    {
//        MasterServer,
//        SlaveServer,
//        ClientLogin,//Used when the client attempts to connect to both master and slave during login
//        ClientHandoff,//Used when the client is instructed to connect to a new slave after a handoff

//    }
//}
