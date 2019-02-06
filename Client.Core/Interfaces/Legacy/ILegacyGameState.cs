using Freecon.Models.TypeEnums;
using Freecon.Client.Mathematics;
using Freecon.Client.Objects.Structures;
using System.Collections.Generic;

namespace Freecon.Client.Core.Interfaces.Legacy
{
    public interface ILegacyGameState : ISynchronousUpdate, IDraw
    {
        Camera2D Camera { get; set; }

        void CreateStructure(float xPos, float yPos, StructureTypes buildingType, float health,
                             float constructionPoints, int ID, HashSet<int> teams);

        Structure GetStructureByID(int structureID);

        void KillStructure(int structureID);

        float Zoom { get; set; }
    }
}
