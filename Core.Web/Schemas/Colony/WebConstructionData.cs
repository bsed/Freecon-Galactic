using Server.Models.Structures;

namespace Core.Web.Schemas
{
    public class WebConstructionData
    {
        public float CurrentConstructionPoints { get; set; }

        public float FullConstructionPoints { get; set; }

        public float TimeRemaining { get; set; }

        public WebStructureInfo StructureInfo { get; set; }

        public WebConstructionData(IConstructionStructureModel structureModel)
        {
            CurrentConstructionPoints = structureModel.CurrentConstructionPoints;
            FullConstructionPoints = structureModel.FullConstructionPoints;
            TimeRemaining = (FullConstructionPoints - CurrentConstructionPoints) / structureModel.ConstructionRate;//Units might be fucked up here, I didn't check what units the rate was in
            StructureInfo = new WebStructureInfo(structureModel);
        
        }
    }
}