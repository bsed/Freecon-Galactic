using Server.Models.Structures;
using System.Collections.Generic;

namespace Server.Models.Interfaces
{
    public interface IHasStructures
    {
        HashSet<int> GetStructureIDs();

        void AddStructure(IStructure s);

        void RemoveStructure(int ID, bool sendRemovalMessage);

        /// <summary>
        /// Places structure in the nearest valid position, checking for overlap with other objects, and snaps the stucture to a grid
        /// </summary>
        /// <param name="structureType"></param>
        /// <param name="xPos"></param>
        /// <param name="yPos"></param>
        /// <returns>True if succesful, false if unable to place the structure</returns>
        bool GetValidStructurePosition(StructureStats stats, ref float xPos, ref float yPos);


    }
}
