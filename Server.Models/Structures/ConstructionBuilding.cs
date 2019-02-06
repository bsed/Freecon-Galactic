using Freecon.Models.TypeEnums;

namespace Server.Models.Structures
{
    /// <summary>
    /// Not fully implemented. Represents a structure which is being constructed
    /// </summary>
    public class ConstructionBuilding : Structure<ConstructionBuildingModel, StructureStats>
    {
      
        public float CurrentConstructionPoints { get { return _model.CurrentConstructionPoints; } set { _model.CurrentConstructionPoints = value; } }
        public float FullConstructionPoints { get { return _model.FullConstructionPoints; } }
        public float ConstructionRate { get { return _model.ConstructionRate; } set { _model.ConstructionRate = value; } }
        public StructureTypes Type { get { return _model.Type; } set { _model.Type = value; } }

        public StructureTypes FinishedType { get { return _model.FinishedType; } set { _model.FinishedType = value; } }

        public ConstructionBuilding()
        { }

        public ConstructionBuilding(ConstructionBuildingModel model)
        {
            _model = model;
        }

        /// <summary>
        /// finishedType is the type of the structure being constructed.
        /// </summary>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <param name="galaxyID"></param>
        /// <param name="owner"></param>
        /// <param name="commandCenter"></param>
        /// <param name="currentAreaID"></param>
        /// <param name="finishedType"></param>
        public ConstructionBuilding(float posX, float posY, int galaxyID, int ownerID, int currentAreaID, StructureTypes finishedType):base(posX, posY, galaxyID, ownerID, currentAreaID)
        {
            _model.StructureType = StructureTypes.ConstructionBuilding;
            _model.FinishedType = finishedType;
        }
        
    }

    public class ConstructionBuildingModel : StructureModel<StructureStats>, IConstructionStructureModel
    {
        public float CurrentConstructionPoints { get; set; }
        public float FullConstructionPoints { get; set; }
        public float ConstructionRate { get; set; }
        public StructureTypes Type { get; set; }
        public StructureTypes FinishedType{get;set;}

        public ConstructionBuildingModel()
        {
            StructureType = StructureTypes.ConstructionBuilding;
        }

        /// <summary>
        /// finishedType is the type of structure being constructed.
        /// </summary>
        /// <param name="finishedType"></param>
        public ConstructionBuildingModel(StructureTypes finishedType)
        {
            Stats = new StructureStats();
            StructureType = StructureTypes.ConstructionBuilding;
            FinishedType = finishedType;

        }
        
        public ConstructionBuildingModel GetClone()
        {
            return (ConstructionBuildingModel)MemberwiseClone();
        }

    }
}
