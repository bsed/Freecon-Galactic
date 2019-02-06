using System.Collections.Generic;
using Freecon.Models.TypeEnums;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using Server.Models.Research;
using Core.Models.Enums;
using Freecon.Models;
using Core.Models;

namespace Server.Models
{
    public class ColonyModel : AreaModel
    {
        public int CommandCenterID { get; set; }
        public int? OwnerTeamID { get; set; }
        public int SystemID { get; set; }
        public int? OwnerID { get; set; }
        

        public string Name { get; set; }

        public override AreaTypes AreaType { get { return AreaTypes.Colony; } }        

        
        public float MaxPowerAvailable { get; set; }

        public bool UseStoredThoriumForPower { get; set; }

        /// <summary>
        /// Need to set these to be updated when the colony updates...
        /// </summary>
        //[BsonDictionaryOptions(DictionaryRepresentation.Document)]
        public Dictionary<ResourceTypes, float> ResourceStockpiles { get; set; } 

        public ResearchHandler ResearchHandler { get; set; }

        public PlanetTypes PlanetType { get; set; }

        public float MoraleAvg { get; set; }
        public float MoraleRateAvg { get; set; }


        public float MaxPopulation { get; set; }
        public float TotalPopulation { get; set; }
        public float PopulationRate { get; set; }



        public float TotalPowerAvailable { get; set; }
        public float PowerInUse { get; set; }

        public string OwnerName { get; set; }


        public Queue<ColonyNewsEvent> NewsEvents { get; set; }

        /// <summary>
        /// In ms
        /// </summary>
        public float TaxPerColonistMs { get { return 1f/24f/60f/60f/1000f; } }

        public float Cash { get; set; }

        public float CashRate { get; set; }//cash per ms

        public float MaxCash {get { return 900000001; } }

        public HashSet<ProblemFlagTypes> ProblemFlags { get; set; }

        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<SliderTypes, Slider> Sliders { get; set; }

        

        public ColonyModel()
        {
            SetupModel();
        }

        public ColonyModel(AreaModel a): base(a)
        {
            SetupModel();
        }

        private void SetupModel()
        {
            Sliders = new Dictionary<SliderTypes, Slider>();
            Sliders.Add(SliderTypes.Construction, new Slider(SliderTypes.Construction, 20));
            Sliders.Add(SliderTypes.Research, new Slider(SliderTypes.Research, 20));
            Sliders.Add(SliderTypes.Mining, new Slider(SliderTypes.Mining, 20));
            Sliders.Add(SliderTypes.Recreation, new Slider(SliderTypes.Recreation, 20));
            Sliders.Add(SliderTypes.Industry, new Slider(SliderTypes.Industry, 20));
            Sliders.Add(SliderTypes.TaxRate, new Slider(SliderTypes.TaxRate, 5));
            ProblemFlags = new HashSet<ProblemFlagTypes>();
        }

        public ColonyModel GetClone()
        {
            return (ColonyModel)MemberwiseClone();
        }
    }
}
