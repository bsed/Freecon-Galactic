using System;
using Freecon.Models.TypeEnums;
using Server.Models.Structures;
using System.Collections.Generic;
using Core.Models.Enums;

namespace Server.Models
{
    /// <summary>
    /// This is where all of the population related data is stored
    /// </summary>
    public class Biodome : ResourceStructure<BiodomeModel, BiodomeStats>
    {        


        public float Morale { get { return _model.Morale; } set { _model.Morale = value; } }
        public float MoraleRate { get { return _model.MoraleRate; } set { _model.MoraleRate = value; } }
        public float MoraleRateMultiplier { get { return _model.MoraleRateMultiplier; } set { _model.MoraleRateMultiplier = value; } }
        public float PlanetPopMultiplier { get { return _model.PlanetBonus; } set { _model.PlanetBonus = value; } }
        public float PopResearchMultiplier { get { return _model.PopResearchBonus; } set { _model.PopResearchBonus = value; } }
        public float Population { get { return _model.Population; } set { _model.Population = value; } }

        public float PopulationRate { get; protected set; }//For display purposes, colonists per hour

        //If true, there are enough resources
        public bool HasPower { get { return _model.HasPower; } }

        public Dictionary<ResourceTypes, Resource> Resources { get { return _model.Resources; } }        

        protected Biodome() { }

        public Biodome(float posX, float posY, int ID, int ownerID, int currentAreaID)
            : base(posX, posY, ID, ownerID, currentAreaID)           
        {
            _model.StructureType = StructureTypes.Biodome;

            ConsumedResources.Add(ResourceTypes.Organics);
            ConsumedResources.Add(ResourceTypes.Medicine);
            ConsumedResources.Add(ResourceTypes.Hydrocarbons);

            MoraleRate = 0;
            Morale = 0;
            
            PlanetPopMultiplier = 1;
            PopResearchMultiplier = 1;
        }

        public Biodome(BiodomeModel bm):base(bm)
        {

        }

        public override float Update(float elapsedMS)
        {
            HashSet<ProblemFlagTypes> newProblemFlags = new HashSet<Core.Models.Enums.ProblemFlagTypes>();

            float resourceBonus = 0;

            //Set resource rates
            _model.Resources[ResourceTypes.Organics].CurrentRate = Population * Stats.OrgRatePerPop;
            _model.Resources[ResourceTypes.Hydrocarbons].CurrentRate = Population * Stats.HydRatePerPop;
            _model.Resources[ResourceTypes.Medicine].CurrentRate = Population * Stats.MedRatePerPop;

            //Consume resources
            foreach(ResourceTypes r in ConsumedResources)
            {
                RemoveResources(r, _model.Resources[r].CurrentRate * elapsedMS);
            }



            if (_model.Resources[ResourceTypes.Organics].NumUnits > 0 && _model.Resources[ResourceTypes.Hydrocarbons].NumUnits > 0)
                resourceBonus = Stats.OrgAndHydPopBonus;
            else if (_model.Resources[ResourceTypes.Organics].NumUnits > 0)
            {
                resourceBonus = Stats.OrgPopBonus;
                newProblemFlags.Add(ProblemFlagTypes.HydrocarbonsDepleted);
            }
            else if (_model.Resources[ResourceTypes.Hydrocarbons].NumUnits > 0)
            {
                resourceBonus = Stats.HydPopBonus;
                newProblemFlags.Add(ProblemFlagTypes.OrganicsDepleted);
            }
            else
                resourceBonus = Stats.MinResourcePopBonus;

            if (_model.Resources[ResourceTypes.Medicine].NumUnits > 0)
            {
                resourceBonus += Stats.MedPopBonus;
            }
            else
            {
                newProblemFlags.Add(ProblemFlagTypes.MedicineDepleted);
            }


            if (!HasPower)
            {
                resourceBonus = Stats.MinResourcePopBonus;//Don't let this go to 0, or population will turn into NaN.
                newProblemFlags.Add(ProblemFlagTypes.NotEnoughPower);
            }
            
            float targetPop = _model.Stats.MaxPopulation * resourceBonus + Morale * Stats.MoraleMultiplier;
            float changeRate = _logGrowthFunction(_model.Population, targetPop, 0.000001f);//Because the elapsed time is measured in milliseconds, the multiplier is tuned to milliseconds.

            //Clamp results. Later I'll implement a quartic growth function, just need to write out a matrix multiplication to define the coefficients in terms of targetPop
            float maxChangeRate = Stats.MaxPopChangeRate / 1000 / 60 / 60;//converted to ms

            if (changeRate > maxChangeRate)
                changeRate = maxChangeRate;
            if (changeRate < -maxChangeRate)
                changeRate = -maxChangeRate;


            //Prevent population growth at 0 or 1
            //We will allow a colony full of inbreds.
            if (Population < 2)
                changeRate = 0;

            if (Population < 0)//Shouldn't be necessary, but just in case, it's here, because bad things will happen if we have a negative population.
                Population = 0;

             
            float changeAmount = changeRate * elapsedMS;
            Population += changeAmount;

            PopulationRate = changeAmount * 1000 * 60 * 60 * PopResearchMultiplier * PlanetPopMultiplier;

            //Need to tweak morale later
            //Assumes Stats.MaxMoraleRate is in units per hour, hence the ms conversion
            MoraleRate = _getMoraleRate(Population, Stats.MaxPopulation, Stats.MaxMoraleRate, resourceBonus) / 1000 / 60 / 60;

            MoraleRate += Math.Abs(MoraleRate) * MoraleRateMultiplier;//Maintain bonus sign

            Morale += MoraleRate * elapsedMS;
            if (Morale >= Stats.MaxMoraleRate)
                Morale = Stats.MaxMoraleRate;
            else if (Morale <= -Stats.MaxMoraleRate)
                Morale = -Stats.MaxMoraleRate;
                
            
            

            _model.Stats.PowerConsumptionRate = Population * Stats.PowerRatePerPop;

            ProblemFlags = newProblemFlags;

            return 0;
            

        }
        /// <summary>
        /// Returns a growth rate, based on a logistic growth function. Maximum rate of positive growth = rateMultiplier(targetPop/2 - targetPop/4)
        /// </summary>
        /// <param name="targetPop">Maximum population that the colony should approach eventually. MUST BE NON ZERO TO AVOID NaN</param>
        /// <param name="rateMultiplier">scales the rate of growth.</param>
        /// <returns></returns>
        float _logGrowthFunction(float currentPop, float targetPop, float rateMultiplier)
        {
            float v = rateMultiplier * currentPop * (1 - (currentPop / targetPop));
            return v;        
        }

        //float _quadraticGrowthFunction(float currentPop, float targetPop, float ratelimiter)
        //{
        //    return ratelimiter * currentPop * (1 - (currentPop / targetPop));
        //}

        float _linearGrowthFunction(float currentPop, float targetPop, float rateMultiplier)
        {
            return -2 * (currentPop - targetPop) * rateMultiplier;
        }    

        /// <summary>
        /// Morale rate incorporating a resource bonus and a crowdedness factor based on a quadratic equation which I pulled out of my ass. Tweak later.
        /// </summary>
        /// <param name="currentPop"></param>
        /// <param name="maxPop"></param>
        /// <param name="maxMoraleRate"></param>
        /// <param name="resourceBonus"></param>
        /// <returns>Morale rate in units of maxMoraleRate</returns>
        float _getMoraleRate(float currentPop, float maxPop, float maxMoraleRate, float resourceBonus)
        {
            //I haven't checked my math, so this might be buggy.
            //Equation should equal -2maxMoraleRate when currentPop = maxPop
            //The idea is that at maximum crowdedness, the morale rate is -maxMoraleRate
            float a = (4 / (maxPop * maxPop)) * (-maxMoraleRate + 1);
            float b = (maxMoraleRate - 2) / maxPop;
            float resBon = resourceBonus * maxMoraleRate;
            float rate = a * currentPop * currentPop + b * currentPop + resBon;

            if (rate > maxMoraleRate)
                rate = maxMoraleRate;
            else if (rate < -maxMoraleRate)
                rate = -maxMoraleRate;

            return rate;

        }
                
    }




    
    public class BiodomeModel:ResourceStructureModel<BiodomeStats>
    {
        //NEED TO CHANGE TO UNITS PER HOUR

        public float Morale { get; set; }
        public float MoraleRate { get; set; }
        public float MoraleRateMultiplier { get; set; }
        public float PlanetBonus { get; set; }
        public float Population { get; set; }
        public float PopResearchBonus { get; set; }//Growth rate bonus from research

        public float Crowdedness { get; set; }

        public bool HasPower { get { return Enabled; } }

        public BiodomeModel()
        {
            StructureType = StructureTypes.Biodome;
            Resources.Add(ResourceTypes.Organics, new Organics());
            Resources.Add(ResourceTypes.Hydrocarbons, new Hydrocarbons());
            Resources.Add(ResourceTypes.Medicine, new Medicine());
        }

        public BiodomeModel GetClone()
        {
            return (BiodomeModel)MemberwiseClone();
        }


    }
}
