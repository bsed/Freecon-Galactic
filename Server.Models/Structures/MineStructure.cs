using Freecon.Models.TypeEnums;
using Server.Models.Space;
using System.Collections.Generic;
using Core.Models.Enums;
using Server.Models.Extensions;
using Server.Managers;

namespace Server.Models.Structures
{
    public class MineStructure : ResourceStructure<MineModel, MineStats>, IResourceSupply
    {       

        public int DroneCount { get { return _model.DroneCount; } set { _model.DroneCount = value; } }
        public int DroneLevel { get { return _model.DroneLevel; } set { _model.DroneLevel = value; } }


        #region Update Variables

        public float SupplyRateAvailableThisUpdate { get; set; }

        public float ThoriumSupplyAvailableThisUpdate;

        #endregion

        /// <summary>
        /// Linked when the mine is registered to the colony. If this is causing null reference exceptions, somebody fucked up.
        /// </summary>
        public IReadOnlyDictionary<ResourceTypes, float> ProductionBonuses { get; set; }


        public float SupplyRatePerDrone = 50;//Units per hour

        public MineStructure()
        { }

        public MineStructure(MineModel model)
        {
            _model = model;
        }

        public MineStructure(float posX, float posY, int galaxyID, int ownerID, int currentAreaID, ICollection<ResourcePool> resourcePools)
            : base(posX, posY, galaxyID, ownerID, currentAreaID)
        { 
            _model.StructureType = StructureTypes.Mine;
           
            _model.Stats.PopulationCost = 50;
            _model.DroneLevel = 1;

            //Set mine rates
            foreach(ResourcePool r in resourcePools)
            {
                
                r.Resource.CurrentRate = _getMineRate(r);
                if(r.Resource.DepletedRate > r.Resource.CurrentRate)
                {
                    r.Resource.DepletedRate = r.Resource.CurrentRate;//Allows some minimum so as not to totally fuck someone for building too far from a resource
                }
                _model.Resources.Add(r.Resource.Type, r.Resource.GetClone());
            }


        }
          

        /// <summary>
        /// Only supplies silos, for now.
        /// </summary>
        /// <param name="elapsedMS"></param>
        /// <param name="resourceSinks"></param>
        public void SupplyUpdate(float elapsedMS, ICollection<IResourceSink> resourceSinks, float sliderValue)
        {
            if (!_model.Enabled)
                return;

                                            
            foreach (var rr in _model.Resources)
            {
                float amountProducedThisUpdate = rr.Value.TakeResource(rr.Value.CurrentRate / 1000 / 60 / 60 * elapsedMS) * sliderValue/100f;

                if (ProductionBonuses != null)//Shouldn't ever be null, but just in case
                {
                    amountProducedThisUpdate *= ProductionBonuses[rr.Key] * ProductionBonuses[ResourceTypes.All];

                }
                else
                {
                    ConsoleManager.WriteLine("ERROR: ProductionBonuses in class MineStructure is null.", ConsoleMessageType.Error);
                }

                if (amountProducedThisUpdate == 0)//Indicates depletion
                    amountProducedThisUpdate = rr.Value.DepletedRate / 1000 / 60 / 60 * elapsedMS;

                //Supply other structures
                foreach (IResourceSink s in resourceSinks)
                {
                    if (s is Silo)
                    {                        
                            _supplyResource(s, rr.Key, ref amountProducedThisUpdate, elapsedMS);

                            if (amountProducedThisUpdate <= 0)//If we've supplied all that we can
                                break;
                        
                    }
                }

            }



        }

        /// <summary>
        /// Supplies the given IResourceSink with the given resource type.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="type"></param>
        /// <param name="supplyAvailableThisUpdate"></param>
        /// <param name="elapsedMS"></param>
        void _supplyResource(IResourceSink s, ResourceTypes type, ref float supplyAvailableThisUpdate, float elapsedMS)
        {

            float supplyAmount = GetSupplyRate(s) * elapsedMS;

            //Make sure supplyAmount doesn't exceed how much can be supplied this update
            if (supplyAmount > supplyAvailableThisUpdate)
            {
                supplyAmount = supplyAvailableThisUpdate;
            }

            float amountUnstored = supplyAmount;
            s.StoreResources(type, ref amountUnstored);
            supplyAvailableThisUpdate -= (supplyAmount - amountUnstored);

        }

        /// <summary>
        /// Linear dropoff with distance for now
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        float _getMineRate(ResourcePool s)
        {
            //This is all kinds of fucked up but I'm sleepy and going to bed.

            float distance = this.DistanceTo(s);

            //Can tweak later, quick and dirty inverse linear function for now
            if (distance > 10)
                return 1f / 10f * Stats.BaseSupplyRate;
            else if (distance <= 1)
                return .1f * Stats.BaseSupplyRate;
            else
                return (-.09f * distance + 10) * Stats.BaseSupplyRate;

        }

        public float GetSupplyRate(IResourceSink s)
        {
            float distance = this.DistanceTo(s);

            //Can tweak later, quick and dirty inverse linear function for now
            if (distance > 10)
                return 1f / 10f * Stats.BaseSupplyRate;
            else if (distance <= 1)
                return .1f * Stats.BaseSupplyRate;
            else
                return (-.09f * distance) * Stats.BaseSupplyRate;
        }

    }

    public class MineModel : ResourceStructureModel<MineStats>
    {
        public int DroneCount { get; set; }
        public int DroneLevel { get; set; }
        

        public MineModel()
        {
            StructureType = StructureTypes.Mine;
        }

        /// <summary>
        /// Can you hide an inherited method if the signature is different?
        /// </summary>
        /// <returns></returns>
        public new MineModel GetClone()
        {
            return (MineModel)MemberwiseClone();
        }

    }
}
