using Core.Models.Enums;
using Freecon.Models.TypeEnums;

namespace Server.Models.Structures
{
    public class PowerPlant : ResourceStructure<PowerPlantModel, PowerPlantStats>, IGenerator, IResourceSink
    {

        public float CurrentPowerRate { get { return _model.CurrentPowerRate; } set { _model.CurrentPowerRate = value; } }
        public float ThoriumPerPower { get { return _model.ThoriumPerPower; } set { _model.ThoriumPerPower = value; } }

        public PowerPlant()
        { }

        public PowerPlant(PowerPlantModel model)
            : base(model)
        { }

        public PowerPlant(float posX, float posY, int galaxyID, int ownerID, int currentAreaID)
            : base(posX, posY, galaxyID, ownerID, currentAreaID)
        {
            _model.StructureType = StructureTypes.PowerPlant;

            _model.Stats.PopulationCost = 50;
            _model.CurrentPowerRate = _model.Stats.MaxPowerGenerationRate;
            _model.ThoriumPerPower = .00001f;//Power is in units/ms

            _model.StoredThorium = 1;
            _model.MaxThorium = 1;

            ConsumedResources.Add(ResourceTypes.Thorium);
            _model.Resources.Add(ResourceTypes.Thorium, new ThoriumOre());
        }     

        /// <summary>
        /// Warning: if elapsedMS is too large, there won't be enough thorium stored to generate power and the plant will be disabled.
        /// </summary>
        /// <param name="elapsedMS"></param>
        /// <returns>Power amount generated this update</returns>
        public float Generate(float elapsedMS)
        {
            if (!Enabled)
                return 0;

            float thoriumConsumed = elapsedMS * CurrentPowerRate * ThoriumPerPower;
            if (_model.StoredThorium > thoriumConsumed)
            {
                _model.StoredThorium -= thoriumConsumed;
                return CurrentPowerRate * elapsedMS;
            }
            else
            {
           
                CurrentPowerRate = _model.StoredThorium * CurrentPowerRate / thoriumConsumed;//Decrease power to a level that supply can keep up with
                _model.StoredThorium -= CurrentPowerRate * ThoriumPerPower * elapsedMS;

                if (CurrentPowerRate <= _model.Stats.MaxPowerGenerationRate * .1f)//If the plant is producing 10% or less of maximum capacity
                {
                    _model.Enabled = false;
                    _model.DisableMessage = "YOU'VE NOT ENOUGH MINERALS (Not enough thorium to continue generating power).";
                    CurrentPowerRate = 0;
                    return 0;
                }

                return CurrentPowerRate * elapsedMS;

            }
            
        }


        /// <summary>
        /// Returns false if full, with the unstored difference returned in numToStore
        /// </summary>
        /// <param name="type"></param>
        /// <param name="numToStore"></param>
        /// <returns></returns>
        public override bool StoreResources(ResourceTypes type, ref float numToStore)
        {
            if (type != ResourceTypes.ThoriumOre)
                return false;

            if (_model.MaxThorium == _model.StoredThorium)
                return false;
            else if (numToStore > ((PowerPlantModel)_model).MaxThorium - ((PowerPlantModel)_model).StoredThorium)
            {
                numToStore -= (((PowerPlantModel)_model).MaxThorium - ((PowerPlantModel)_model).StoredThorium);
                ((PowerPlantModel)_model).StoredThorium = ((PowerPlantModel)_model).MaxThorium;

            }
            else if (numToStore < ((PowerPlantModel)_model).MaxThorium - ((PowerPlantModel)_model).StoredThorium)
            {
                ((PowerPlantModel)_model).StoredThorium += numToStore;
                numToStore = 0;

            }

            return (((PowerPlantModel)_model).MaxThorium != ((PowerPlantModel)_model).StoredThorium);

        }   
                  
    }

    public class PowerPlantModel : ResourceStructureModel<PowerPlantStats>
    {
        public float CurrentPowerRate { get; set; }
        public float ThoriumPerPower { get; set; }//Units thorium consumed per unit power generated

        public float StoredThorium { get; set; }
        public float MaxThorium { get; set; }

        public PowerPlantModel()
        {
            StructureType = StructureTypes.PowerPlant;
        }

        new public PowerPlantModel GetClone()
        {
            return (PowerPlantModel)MemberwiseClone();
        }

    }
}
