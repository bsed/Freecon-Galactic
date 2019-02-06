using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Freecon.Models.TypeEnums;

namespace Client.View.JSMarshalling
{
    public class StructurePlacementRequest:JSMarshallContainer
    {
        public StructureTypes StructureType;

        public float PosX, PosY;

        public StructurePlacementRequest()
        {
            MethodName = "StructurePlacementRequest";
        }
    }
}
