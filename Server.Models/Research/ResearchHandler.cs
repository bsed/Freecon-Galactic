using Server.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using Core.Models.Enums;
using Freecon.Core.Utils;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace Server.Models.Research
{
    public class ResearchHandler
    {
        public int DBId { get; set; }

        //[BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public static HashSet<ResearchDiscoveries> AllDiscoveries { get; set; }

        static int MaxResearchLevel { get; set; }

        //[BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public HashSet<ResearchDiscoveries> Discoveries { get; set; }

        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<ResearchDiscoveries, int> DiscoveryLevels { get; set; }

        #region Bonuses
        //Bonuses        
        public float GrowthRateMultiplier {get; protected set;}
        public float MoraleRateMultiplier {get; protected set;}
        public float TaxRevenueMultiplier {get; protected set;}
        public float ConstructionRateMultiplier { get; protected set;}//TODO: implement
        public float StructureDamageMultiplier { get; protected set;}
        public float PowerGenerationMultiplier {get; protected set;}
        public float PopulationUsageMultiplier {get; protected set;}//Reduces the population required to run buildings

        public bool PropagandaDiscovered { get; protected set; }


        //[BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        [BsonIgnore]
        public IReadOnlyDictionary<ResourceTypes, float> ProductionBonuses { get { return (IReadOnlyDictionary<ResourceTypes, float>)_productionBonuses; } }

        //[BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        Dictionary<ResourceTypes, float> _productionBonuses;

        #endregion

        static ResearchHandler()
        {
            AllDiscoveries = new HashSet<ResearchDiscoveries>(Enum.GetValues(typeof(ResearchDiscoveries)).Cast<ResearchDiscoveries>());
            MaxResearchLevel = 5;
        }

        public ResearchHandler()
        {
            Discoveries = new HashSet<ResearchDiscoveries>();
            DiscoveryLevels = new Dictionary<ResearchDiscoveries, int>();
            _productionBonuses = new Dictionary<ResourceTypes, float>();
            foreach (var v in Enum.GetValues(typeof(ResourceTypes)).Cast<ResourceTypes>())
                _productionBonuses.Add(v, 1);
        }

        /// <summary>
        /// Returns null if no research is discovered.
        /// </summary>
        /// <param name="elapsedMS">In milliseconds</param>
        /// <param name="currentPopulation"></param>
        /// <param name="numResearchFacilities"></param>
        /// <param name="researchFacilityUtilization">Average percentage of researchFacilityUtilization, expressed as a percentage</param>
        public ResearchResult Update(float elapsedMS, float currentPopulation, float numResearchFacilities, float researchFacilityUtilization)
        {     

            float probabilityPerMS = _probabilityOfDiscovery(Discoveries.Count, currentPopulation, (numResearchFacilities+1) * researchFacilityUtilization/100f);
            float pDiscovery = probabilityPerMS * elapsedMS;            

            if(Rand.Random.NextDouble() < pDiscovery)//Discovery!
            {
                return _makeDiscovery();
            }

            return null;
        
        }

        /// <summary>
        /// Chooses and adds a new or levels up an existing discovery
        /// </summary>
        ResearchResult _makeDiscovery()
        {
            float pNewDiscovery = .30f;//66% chance that an existing discovery will be leveled as opposed to finding a new discovery 

            //Roll for new discovery
            if(Rand.Random.NextDouble() < pNewDiscovery || Discoveries.Count() == 0)
            {
                if (_tryNewDiscovery() == null)
                    return _tryUpgradeDiscovery();
            }
            else
            {
                return _tryUpgradeDiscovery();
            }

            return null;
        }

        /// <summary>
        /// Returns true if a new discovery is succesfully made and added to the ResearchHandler
        /// </summary>
        /// <returns></returns>
        ResearchResult _tryNewDiscovery()
        {
            IEnumerable<ResearchDiscoveries> remainingDiscoveries = AllDiscoveries.Except(Discoveries);
            if (remainingDiscoveries.Count() != 0)
            {
                ResearchDiscoveries s = remainingDiscoveries.ElementAt(Rand.Random.Next(0, remainingDiscoveries.Count()));
                Discoveries.Add(s);
                DiscoveryLevels.Add(s, 1);

#if DEBUG
                ConsoleManager.WriteLine("Discovered " + s.ToString(), ConsoleMessageType.Debug);
#endif


                return new ResearchResult() { Discovery = s, Level = 1 };
            }
            else
            {
                return null;
            }

        }

        /// <summary>
        /// Returns true if an existing discovery is succesfully selected and upgraded
        /// </summary>
        /// <returns></returns>
        ResearchResult _tryUpgradeDiscovery()
        {
            var nonMaxxed = DiscoveryLevels.Where(a => (a.Value < ResearchHandler.MaxResearchLevel));
            if(nonMaxxed.Count() != 0)
            {                
                var resToUpgrade = nonMaxxed.ElementAt(Rand.Random.Next(0, nonMaxxed.Count()));
                DiscoveryLevels[resToUpgrade.Key] += 1;

#if DEBUG
                ConsoleManager.WriteLine("Discovered " + resToUpgrade.Key.ToString() + " level " + DiscoveryLevels[resToUpgrade.Key], ConsoleMessageType.Debug);
#endif

                return new ResearchResult() { Discovery = resToUpgrade.Key, Level = resToUpgrade.Value };
            }
            else
            {
                return null;
            }
        }


        public void ResetBonuses()
        {

            GrowthRateMultiplier = 1;
            MoraleRateMultiplier = 1;
            TaxRevenueMultiplier = 1;
            ConstructionRateMultiplier = 1;
            StructureDamageMultiplier = 1;
            PowerGenerationMultiplier = 1;
            PopulationUsageMultiplier = 1;

            
            

            foreach(ResearchDiscoveries s in Discoveries)
            {
                switch(s)
                {
                    case(ResearchDiscoveries.SiteDesign):
                        {
                            ConstructionRateMultiplier += DiscoveryLevels[ResearchDiscoveries.SiteDesign] * .1f;
                            break;
                        }
                    case(ResearchDiscoveries.EnvironmentalResistance):
                        {
                            StructureDamageMultiplier -= DiscoveryLevels[ResearchDiscoveries.EnvironmentalResistance] * .05f;
                            break;
                        }
                    case(ResearchDiscoveries.DefensiveEmplacement):
                        {
                            StructureDamageMultiplier -= DiscoveryLevels[ResearchDiscoveries.DefensiveEmplacement] * .05f;
                            break;
                        }
                    case (ResearchDiscoveries.MicroClimateControl):
                        {
                            StructureDamageMultiplier -= DiscoveryLevels[ResearchDiscoveries.EnvironmentalResistance] * .05f;
                            break;
                        }
                    case(ResearchDiscoveries.MineDesign):
                        { 

                            _productionBonuses[ResourceTypes.All] += DiscoveryLevels[ResearchDiscoveries.MineDesign] * .1f;

                            break;
                        }
                    case(ResearchDiscoveries.TaxDistribution):
                        {
                            TaxRevenueMultiplier += DiscoveryLevels[ResearchDiscoveries.TaxDistribution] * .1f; 
                            break;
                        }
                    case(ResearchDiscoveries.CentralPlanning ):
                        {
                            TaxRevenueMultiplier += DiscoveryLevels[ResearchDiscoveries.CentralPlanning] * .1f; 
                            break;
                        }
                    case(ResearchDiscoveries.FreeMarkets):
                        {
                            TaxRevenueMultiplier += DiscoveryLevels[ResearchDiscoveries.FreeMarkets] * .1f; 
                            break;
                        }
                    case(ResearchDiscoveries.EconomicPropaganda):
                        {
                            TaxRevenueMultiplier += DiscoveryLevels[ResearchDiscoveries.EconomicPropaganda] * .1f;
                            MoraleRateMultiplier += .05f;
                            break;
                        }                    
                    case(ResearchDiscoveries.PsychiatricEpidemiology):
                        {
                            GrowthRateMultiplier += DiscoveryLevels[ResearchDiscoveries.PsychiatricEpidemiology] * .1f;
                            MoraleRateMultiplier += DiscoveryLevels[ResearchDiscoveries.PsychiatricEpidemiology] * .05f;
                            break;
                        }
                    case(ResearchDiscoveries.PsychologicalProfiling):
                        {
                            MoraleRateMultiplier += DiscoveryLevels[ResearchDiscoveries.PsychologicalProfiling] * .1f;
                            break;
                        }
                    case(ResearchDiscoveries.Propaganda):
                        {
                            PropagandaDiscovered = true;
                            break;
                        }
                    case(ResearchDiscoveries.RedLightDistrict):
                        {
                            GrowthRateMultiplier += DiscoveryLevels[ResearchDiscoveries.RedLightDistrict] * 1f; //AWW YEAH GET YO FREAK ON FAM
                            MoraleRateMultiplier += DiscoveryLevels[ResearchDiscoveries.RedLightDistrict] * 1f; //YEAH BOY ERREYBODY FEELIN GOOD
                            break;
                        }


                }
            }
        
        }

        /// <summary>
        /// Returns the probability of discovery, per ms
        /// </summary>
        /// <param name="numDiscoveries"></param>
        /// <param name="population"></param>
        /// <param name="facilityBonusMultiplier">Time to discovery is divided by this value. CANNOT BE 0</param>
        /// <returns></returns>
        float _probabilityOfDiscovery(int numDiscoveries, float population, float facilityBonusMultiplier)
        {
            float hoursToDiscovery = 0;//Readability
            float colonistMultiplier = 0;


            if (numDiscoveries < 3)
            {
                hoursToDiscovery = 12;

            }
            else
            {
                //Double discovery time for every 2 discoveries
                hoursToDiscovery = 12 * (float)Math.Floor(numDiscoveries / 2f);
            }

            if (facilityBonusMultiplier <= 0)
                throw new Exception("facilityBonusMultiplier cannot be 0");

            hoursToDiscovery /= facilityBonusMultiplier;


            //Population multiplier is linear until 1000 where it equals 1 and is constant
            if (population == 0)
                hoursToDiscovery = 9999999999;
            else if (population < 1000)
                colonistMultiplier = 0.001f * population;
            else
                colonistMultiplier = 1;


            return (1 / hoursToDiscovery) / 60 / 60 / 1000; //Conversion to ms

        }
    }

    public class ResearchResult
    {
        public ResearchDiscoveries Discovery;
        public int Level;

    }


}
