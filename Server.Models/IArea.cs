using Freecon.Models.TypeEnums;
using Server.Interfaces;
using Server.Models.Interfaces;
using Server.Models.Space;
using Server.Models.Structures;
using System.Collections.Generic;
using Freecon.Core.Networking.Models;
using Freecon.Core.Interfaces;

namespace Server.Models
{
    public interface IArea : IHasGalaxyID, ISerializable, IHasPosition
    {       
        Dictionary<int, Player> GetOnlinePlayers();        
        List<int> GetShipIDs();
        Dictionary<int, IShip> GetShips();
        float PosX { get; }
        float PosY { get; }
        bool GetValidStructurePosition(StructureStats stats, ref float xPos, ref float yPos);
        int Id { get; set; }
        int NumOnlinePlayers { get; }
        int NumOnlineHumanPlayers { get; }
        int NumNPCs { get; }
        int NumShips { get; }
        List<ResourcePool> ResourcePools { get; set; }

        IReadOnlyCollection<int> ShipIDs { get; }
        IReadOnlyCollection<int> OnlinePlayerIDs { get; }

        int? ParentAreaID { get; set; }
        byte SecurityLevel { get; set; }

        AreaTypes AreaType { get; }
        string AreaName { get; set; }
        int IDToOrbit { get; set; }

        int AreaSize { get; set; }
        int CurrentTrip { get; set; }
        int Distance { get; set; }
        int MaxTrip { get; set; }
        List<Warphole> Warpholes { get; set; }

        
        /// <summary>
        /// Does not move the ship. Does not set CurrentAreaID.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="isWarping"></param>
        void MovePlayerHere(Player p, bool isWarping);
        
        void MoveShipHere(NPCShip npc);
        void MoveShipHere(IShip s);
        void MoveShipHere(IShip s, float xPos, float yPos);
        void RemovePlayer(Player p);
        void RemoveShip(NPCShip npc);
        void RemoveShip(IShip s);
        void RemoveStructure(int ID, bool sendRemovalMessage);
        void SetEntryPosition(IHasPosition warpingObject, IArea oldArea);
        void SendEntryData(HumanPlayer client, bool warping, IShip playerShip);
        void ShipFired(IShip firingShip, float rotation, byte weaponSlot, List<int> projectileIDs, IProjectileManager pm, byte pctCharge = 0);
        bool TryFireStructure(int firingStructureID, float rotationOfIncoming, List<int> projectileIDs, IProjectileManager pm, byte pctCharge, byte weaponSlot);
        void StructureFired(ICanFire firingStructure, float rotation, List<int> projectileIDs, IProjectileManager pm, byte percentCharge, byte weaponSlot);
        void Update(float currentTime);

        void AddPlayer(Player p, bool isWarping);
        void AddShip(NPCShip npc, bool suspendNetworking);
        void AddShip(IShip s, bool suspendNetworking=false);
        void AddStructure(IStructure s);
        IStructure GetStructure(int structureID);
        bool CanAddStructure(Player player, StructureTypes buildingType, float xPos, float yPos, out string resultMessage);

        /// <summary>
        /// Gets the object with the corresponding ID, if it exists
        /// </summary>
        /// <param name="objectID"></param>
        /// <returns></returns>
        IFloatyAreaObject GetFloatyAreaObject(int objectID);

        /// <summary>
        /// Adds specified objects to area.
        /// Removes the objects and sends notification messages as appropriate
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        void AddFloatyAreaObjects(IEnumerable<IFloatyAreaObject> objects);
        
        /// <summary>
        /// Removes the objects and sends notification messages as appropriate
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        void RemoveFloatyAreaObjects(IEnumerable<int> objectIDs);

        IArea GetParentArea();

        IReadOnlyDictionary<int, IStructure> GetStructures();

        HashSet<int> GetStructureIDs();

        /// <summary>
        /// Checks if a ship can warp from this area to the given areaID
        /// </summary>
        /// <param name="areaID"></param>
        /// <returns></returns>
        bool IsAreaConnected(int areaID);

        void BroadcastMessage(NetworkMessageContainer message, int? playerIDToIgnore = null, bool sendToHumanClientsOnly = false);
    }
}
