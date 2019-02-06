using System.Collections.Generic;
using Freecon.Models.TypeEnums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Server.Models.Structures;

namespace Core.Web.Schemas
{
    public class WebStructureInfo
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public StructureTypes StructureType { get; set; }

        public string Description { get; set; }

        public WebStructureInfo(IStructureModel structureModel)
        {
            StructureType = structureModel.StructureType;
            Description = TypeToDescription[structureModel.StructureType];
        }

        public static Dictionary<StructureTypes, string> TypeToDescription = new Dictionary<StructureTypes, string>()
        {
            { StructureTypes.Biodome, "People live here and stuff"},
            { StructureTypes.CommandCenter, "This is where the facists live"},
            { StructureTypes.ConstructionBuilding, "If you build it, they will come"},
            { StructureTypes.Factory, "Oppressed proletariat works here"},
            { StructureTypes.LaserTurret, "This guards the gold"},
            { StructureTypes.Mine, "This is where the children mine the gold"},
            { StructureTypes.PowerPlant, "This is where meltdowns happen"},
            { StructureTypes.Refinery, "This is where they make the drugs"},
            { StructureTypes.Silo, "This is where they hide the drugs"},


        };
    }
}