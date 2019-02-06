using System.Collections.Generic;
using Core.Models.Enums;

namespace Server.Models.Research
{

    public class ResearchManager
    {
        /// <summary>
        /// Discoveries are modeled as bernoulli trials, where on each update there is a boolean chance that a discovery is made.
        /// The average time to discovery is given by dt/p, where p is the probability of discovery per unit time, and dt is the time between trials(updates)
        /// </summary>
        Dictionary<ResearchDiscoveries, float> _relativeProbabilities { get; set; }






        public ResearchManager()
        {
            _relativeProbabilities.Add(ResearchDiscoveries.CentralPlanning, 1);
            _relativeProbabilities.Add(ResearchDiscoveries.DefensiveEmplacement, 1);
            _relativeProbabilities.Add(ResearchDiscoveries.EconomicPropaganda, 1);
            _relativeProbabilities.Add(ResearchDiscoveries.EnvironmentalResistance, 1);
            _relativeProbabilities.Add(ResearchDiscoveries.FreeMarkets, 1);
            _relativeProbabilities.Add(ResearchDiscoveries.MicroClimateControl, 1);
            _relativeProbabilities.Add(ResearchDiscoveries.MineDesign, 1);
            _relativeProbabilities.Add(ResearchDiscoveries.PlanetaryComposition, 1);
            _relativeProbabilities.Add(ResearchDiscoveries.PlanetaryStructure, 1);
            _relativeProbabilities.Add(ResearchDiscoveries.PlateTectonics, 1);
            _relativeProbabilities.Add(ResearchDiscoveries.Propaganda, 1);
            _relativeProbabilities.Add(ResearchDiscoveries.PsychiatricEpidemiology, 1);
            _relativeProbabilities.Add(ResearchDiscoveries.PsychologicalProfiling, 1);
            _relativeProbabilities.Add(ResearchDiscoveries.SiteDesign, 1);
            _relativeProbabilities.Add(ResearchDiscoveries.SoilCharacterization, 1);
            _relativeProbabilities.Add(ResearchDiscoveries.TaxDistribution, 1);
            
        }        
        
        

        
    }
}
