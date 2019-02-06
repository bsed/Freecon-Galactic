using System.Collections.Generic;
using Freecon.Models.TypeEnums;
using SRServer.Services;
using Server.Models.Interfaces;
using Server.Interfaces;
using Server.Managers;


namespace Server.Models.Space
{
    /// <summary>
    /// Limbo is a temporary place to store ships and players during area changes; Probably obsolete and to be removed soon
    /// </summary>
    public class Limbo : Area<LimboModel>
    {
        protected Limbo() { }

        public Limbo(int ID, LocatorService ls):base(ID, ls)
        {

        }
       
        public override void AddShip(IShip s, bool suspendNetworking)
        {
            _model.ShipIDs.Add(s.Id);
            _shipCache.Add(s.Id, s); 
        }

        public override void AddShip(NPCShip npc, bool suspendNetworking)
        {
            _model.ShipIDs.Add(npc.Id);
            _shipCache.Add(npc.Id, npc);
          
        }

        public override void RemoveShip(IShip s)
        {  
            _model.ShipIDs.Remove(s.Id);
            _shipCache.Remove(s.Id);
        }

        public override void RemoveShip(NPCShip npc)
        { 
            ISimulatable temp;
            _model.ShipIDs.Remove(npc.Id);
            _shipCache.Remove(npc.Id);
            SimulatableObjects.TryRemove(npc.Id, out temp);
        }


        public override void SendEntryData(HumanPlayer sendHere, bool warping, IShip playerShip)
        {
           
        }


        public override void ShipFired(IShip firingShip, float rotation, byte weaponSlot, List<int> projectileIDs, IProjectileManager pm, byte pctCharge = 0)
        {           

        
        }

        public override bool CanAddStructure(Player player, StructureTypes buildingType, float xPos, float yPos, out string resultMessage)
        {
            resultMessage = "";
            ConsoleManager.WriteLine("CanAddStructure called for a player that was in limbo. wut", ConsoleMessageType.Error);
            return false;
        }

        
    }


    public class LimboModel:AreaModel
    {
        public override AreaTypes AreaType { get { return AreaTypes.Limbo; } }

    }

}
