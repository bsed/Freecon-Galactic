using System.Collections.Generic;
using Core.Models.Enums;

namespace Server.Balancers
{
    public class ResourceBalancer
    {
        //Resources going into the system
        public float ExploitationRatePerSpawn;

        public float PlanetSpawnRate;

        public float PortSpawnRate;

        public float ExploitationPerPortSpawnRate;

        public float NPCSpawnRate;

        public float NPCExploitationRate;

        public IReadOnlyDictionary<ResourceTypes, float> RelativeAbundances { get { return (IReadOnlyDictionary<ResourceTypes, float>)_relativeAbundances; } }
        Dictionary<ResourceTypes, float> _relativeAbundances;

        public ResourceBalancer(ResourceBalancerConfig config)
        {

            _relativeAbundances.Add(ResourceTypes.ThoriumOre, 1);
            _relativeAbundances.Add(ResourceTypes.Hydrogen, 2);
            _relativeAbundances.Add(ResourceTypes.Cash, 10000);
            _relativeAbundances.Add(ResourceTypes.IronOre, 50);
            _relativeAbundances.Add(ResourceTypes.Organics, 2);
            _relativeAbundances.Add(ResourceTypes.Hydrocarbons, 2);
            _relativeAbundances.Add(ResourceTypes.Bauxite, 4);
            _relativeAbundances.Add(ResourceTypes.Silica, 100);
            _relativeAbundances.Add(ResourceTypes.Water, 100);
            
            

        }



        #region Sources
        
        float SpawnRate()
        {
            float planetSpawnRate;
            float resourcesPerPlanet;
            return 0;

        }
        
        float MineRate()
        {
            float numPlanets = 0;
            float coloniesPerPlanet = 0;
            float minesPerColony = 0;           
            float ratePerMine = 0;





            return numPlanets * coloniesPerPlanet * minesPerColony * ratePerMine;
        }

        float NPCRate()
        {
            return 0;
        }
              
        
        #endregion


        #region Sinks

        float DeathRate()
        {
            return 0;
        }

        float SellRate()
        {
            return 0;
        }

        float BuildRate()
        {
            return 0;
        }

        /// <summary>
        /// Consumption by colonies
        /// </summary>
        /// <returns></returns>
        float ColonyRate()
        {
            return 0;
        }

        #endregion








    }

    public class ResourceBalancerConfig
    {
        


    }
}
