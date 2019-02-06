namespace Freecon.Core.Networking.Models
{
    /// <summary>
    /// Holds all relavent message headers.
    /// </summary>
    public enum MessageTypes : byte
    {
        // Updates
        ClientHeartbeat,
        ServerHeartbeat,

        //Requests
        ShipFireRequest,
        StructureFireRequest,
        WarpRequest,
        LandRequest,
        DockRequest, //For docking with things in space, like ports, bases, motherships
        UndockRequest,
        PurchaseRequest, //Request to purchase from port, base, etc
        LeaveToPlanetRequest,//Leave a colony to the planet surface
        LeaveToSpaceRequest,//Leave a colony to space
        EnterColonyRequest,
        StructurePlacementRequest,
        PortTradeRequest,
        ShipTradeRequest,

        // Request Responses
        WarpApproval,
        WarpDenial,
        PlanetLandApproval,
        FireRequestResponse,     
        ColonizeRequestApproval,
        ColonizeRequestDenial,
        ColonyEntryDenial,
        StructurePlacementResponse,
        PortTradeResponse,

        // Server Updates
        ObjectFired,//Object fired a weapon
        ReceiveNewShips,
        PositionUpdateData,
        StarSystemData,
        AddCargoToShip,
        RemoveCargoFromShip,

        // Login Related
        ClientLoginSuccess,
        ClientLoginFailed,

        //Commands
        ChangeShipType,
        EnterColony,
        SetHealth,
        ChangeShipLocation,
        PortDockApproval, //Contains port data
        ReceiveNewPortShip,
        ReceiveNewStructure,
        RemoveKillRevive,
        InitiateShipTrade,
        CancelShipTrade,

        // Other
        AddRemoveShipsTeam,//Add or remove ships from/to team
        ChatMessage,
        AdminCommand,
        ProjectileCollisionReport,
        ReceiveFloatyAreaObjects,//New objects have entered the area
        ObjectPickupRequest,
        TradeUpdateData,//Update of items offered

        NullType,
        ClientException,
        SelectorMessageType,
        ColonyCaptured,

        CheatNotification,
        TimeGet,
        TimeSync,


        //Redis Only
        Redis_AdminWarpPlayer,
        Redis_ClientHandoff,
        Redis_SimulatorMessage,
        Redis_PlayerRemoveTeam,
        Redis_Shout,
        Redis_Login,
        Redis_LoginDataRequest,
        Redis_LoginDataResponse,
        Redis_ColonyDataPush,
        Redis_IDRequest,
        Redis_IDResponse,
        Redis_StartUpdatingSystems,
        Redis_SlaveConnectionRequest,
        Redis_SlaveConnectionResponse,
        Redis_SimulatorConnectionRequest,
        Redis_SimulatorConnectionResponse,
        Redis_NumOnlinePlayersChanged,//Notifies simulator to change update rate and syncs number of online players in an area
        Redis_SlaveDisconnectionDetected,//Sent to the slave if the master server detects a disconnection, to allow the slave to gracefully shutdown if it hasn't actually crashed.
    }

    public enum RedisChatTypes
    {
        Admin,
        Shout,
        ToPlayer,
        ToList
    }

    public enum SelectorCommands : byte
    {
        HoldPosition,
        GoToPosition,
        AttackToPosition,
        Stop,
        AttackTarget,

    }

    public enum AdminCommands : byte
    {
        setShip,
        systemStatistics,
        killPlayer,
        whoplayer,
        allyNPCs,
        makeNPCs
    }
}