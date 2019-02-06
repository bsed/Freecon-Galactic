using System.Collections.Generic;
using Freecon.Models.TypeEnums;

namespace Freecon.Core.Networking.Models.Objects
{
    public class StructureData
    {
        public StructureTypes StructureType;
        public float XPos;
        public float YPos;
        public float CurrentHealth;
        public int Id;
        public HashSet<int> OwnerTeamIDs;

        public StructureData()
        {
            OwnerTeamIDs = new HashSet<int>();
        }

        public StructureData(StructureData d)
        {
            CurrentHealth = d.CurrentHealth;
            Id = d.Id;
            StructureType = d.StructureType;
            XPos = d.XPos;
            YPos = d.YPos;
        }
        
    }
}