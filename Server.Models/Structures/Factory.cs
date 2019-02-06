using Core.Models.CargoHandlers;
using Freecon.Models.TypeEnums;
using Server.Models.Interfaces;
using System.Collections.Generic;

namespace Server.Models.Structures
{
    public class Factory : Structure<FactoryModel, FactoryStats>, IHasCargo
    {
        /// <summary>
        /// Points required to construct each object type
        /// </summary>
        static IReadOnlyDictionary<Constructables, float> ConstructionPoints;

        public float ConstructionRateMultiplier { get; set; }

        public CargoHandler_ReadAddVM<CargoHandlerModel> Cargo { get; protected set; }

        /// <summary>
        /// StructureManager.Update() checks if this is not default (null), and instantiates the object, adding it to the cargo, as necessary. No construction will occur so long as this is not default (null)
        /// </summary>
        public Constructables CompletedPendingInstantiation { get { return _model.CompletedPendingInstantiation; } set { _model.CompletedPendingInstantiation = value; } }

        static Factory()
        {
            Dictionary<Constructables, float> constructionPoints = new Dictionary<Constructables, float>();
            constructionPoints.Add(Constructables.HellHoundMissile, 100);
            constructionPoints.Add(Constructables.AmbassadorMissile, 100);
            constructionPoints.Add(Constructables.Biodome, 500);
            constructionPoints.Add(Constructables.CargoHold, 50);
            constructionPoints.Add(Constructables.LaserTurret, 200);
            constructionPoints.Add(Constructables.Module1, 500);
            constructionPoints.Add(Constructables.Module2, 600);
            constructionPoints.Add(Constructables.Module3, 700);
            constructionPoints.Add(Constructables.Weapon1, 500);
            constructionPoints.Add(Constructables.Weapon2, 600);
            constructionPoints.Add(Constructables.Weapon3, 700);
            constructionPoints.Add(Constructables.Hull1, 500);
            constructionPoints.Add(Constructables.Hull2, 600);
            constructionPoints.Add(Constructables.Hull3, 700);
            constructionPoints.Add(Constructables.Engine1, 500);
            constructionPoints.Add(Constructables.Engine2, 600);
            constructionPoints.Add(Constructables.Engine3, 700);
            ConstructionPoints = constructionPoints;
            
        }

        public Factory()
        {
            _model = new FactoryModel();
        }

        public Factory(float posX, float posY, int galaxyID, int ownerID, int currentAreaID):base(posX, posY, galaxyID, ownerID, currentAreaID)
        {
            _model = new FactoryModel();
            _model.StructureType = StructureTypes.Factory;
            Cargo = new CargoHandler_ReadAddVM<CargoHandlerModel>();
            Cargo.TotalHolds = 100000;//Might change to a custom view model to limit quantities of individual objects
        }

        public Factory(FactoryModel model):base(model)
        { }
        
        public CargoHandler_ReadOnlyVM<CargoHandlerModel> GetCargo()
        {
            return new CargoHandler_ReadOnlyVM<CargoHandlerModel>(Cargo);
        }
        
        public override float Update(float elapsedMS)
        {

            float pointsThisUpdate = Stats.BaseConstructionRate * ConstructionRateMultiplier * elapsedMS;

            if(_model.CompletedPendingInstantiation == Constructables.Null)
            {
                ConstructionObject currentObject = _model.ConstructionQueue.Peek();
                currentObject.CurrentPoints += pointsThisUpdate;
            
                if(currentObject.CurrentPoints >= ConstructionPoints[currentObject.Type])
                {
                    _model.CompletedPendingInstantiation = currentObject.Type;
                }


            }
            
            

            return 0;
        }
        
    }

    public class FactoryModel : StructureModel<FactoryStats>
    {
        internal Queue<ConstructionObject> ConstructionQueue { get; set; }

        protected CargoHandler_ReadOnlyVM<CargoHandlerModel> Cargo { get; set; }

        public float ConstructionRateMultiplier { get; set; }

        public Constructables CompletedPendingInstantiation { get; set; }

        public FactoryModel()
        {
            StructureType = StructureTypes.Factory;
            ConstructionQueue = new Queue<ConstructionObject>();
            CompletedPendingInstantiation = Constructables.Null;
        }

        

        public FactoryModel(StructureModel<FactoryStats> s)
            : base(s)
        { }

    }

   
    
    /// <summary>
    /// Represents cargo which is under construction
    /// </summary>
    internal class ConstructionObject
    {
        public Constructables Type { get; set; }

        public float CurrentPoints { get; set; }

    }

    public enum Constructables
    {
        Null = 0,//Default value

        LaserTurret,

        Module1,
        Module2,
        Module3,

        Weapon1,
        Weapon2,
        Weapon3,

        Hull1,
        Hull2,
        Hull3,

        Engine1,
        Engine2,
        Engine3,

        AmbassadorMissile,
        HellHoundMissile,
        Biodome,
        CargoHold,

        
    }
}
