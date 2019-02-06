using Freecon.Models.TypeEnums;
using Server.Models.Structures;
using System.Collections.Generic;

namespace Server.Managers
{
    /// <summary>
    /// Under construction. Need to change to match ShipStats implementation, including DB setup
    /// </summary>
    public class StructureStatManager
    {
        static Dictionary<StructureTypes, StructureStats> _stats;



        public static void Initialize()
        {
            _stats = new Dictionary<StructureTypes, StructureStats>();
            _stats.Add(StructureTypes.LaserTurret, new TurretStats());
            _stats.Add(StructureTypes.Mine, new MineStats());
        }

        
        /// <summary>
        /// Returns a clone of the appropriate StructureStats object
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static StructureStats GetStats(StructureTypes s)
        {
            if(_stats.ContainsKey(s))
            {
                return _stats[s].GetClone();
            }
            else
            {
                ConsoleManager.WriteLine("Error: " + s.ToString() + " not found in StructureStatManager.GetStats.", ConsoleMessageType.Error);
                return null;
            }

        }


    }   
    
    
    
    
    
   



}
