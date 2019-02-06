using Freecon.Core.Interfaces;
using Freecon.Models.TypeEnums;

namespace Server.Models.Structures
{
    public interface IStructureModel: IDBObject
    {
        StructureTypes StructureType { get; }

        int? OwnerID { get; set; }

        int? CurrentAreaID { get; }

        bool IsDead { get; set; }

        bool Enabled { get; }

        float XPos { get; set; }

        float YPos { get; set; }

        float CurrentHealth { get; set; }
    
    }
}