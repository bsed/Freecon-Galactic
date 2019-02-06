using System.Collections.Generic;
using Core.Web.Schemas.Components;
using Core.Web.Schemas.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Server.Models;
using Core.Models.Enums;
using Core.Models;
using Freecon.Models;
using Server.Models.Structures;

namespace Core.Web.Schemas
{

    /// <summary>
    /// The data used to build the client's colony overview.
    /// </summary>
    public class ColonyOverviewPageData : IColonyPage
    {
        public List<StatDisplay> StatDisplays { get; set; }

        public IEnumerable<Slider> Sliders { get; set; }

        public WebStructuresOverviewData WebStructuresOverviewData { get; set; }

        public IEnumerable<ColonyNewsEvent> ColonyEvents { get; set; }

        public List<StatusIndicatorData> StatusIndicators { get; set; } 

        public string PageName { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ColonyPages PageType { get; set; }

        public bool IsEnabled { get; set; }

        public int Order { get; set; }
        
        public ColonyOverviewPageData(ColonyModel cm, IEnumerable<IStructureModel> structureModels)
        {
            StatDisplays = new List<StatDisplay>();

            IsEnabled = true;
            Order = 0;
            PageName = "Overview";
            PageType = ColonyPages.Overview;

            ColonyEvents = cm.NewsEvents;

            var indicators = new List<StatusIndicatorData>();
            foreach(var pf in cm.ProblemFlags)
            {
                indicators.Add(new StatusIndicatorData(pf));   
            }

            //Population
            StatDisplays.Add(new StatDisplay
            {
                Type = StatDisplayTypes.Population,
                DisplayName = "Population",
                CurrentValue = cm.TotalPopulation,
                RateOfChange = cm.PopulationRate,
                MaxValue = cm.MaxPopulation,
                TimeUnit = TimeUnits.Hour,
                Tooltip = "The population of the colony. This indicates the size of the colony."
            });
            
            //Morale
            StatDisplays.Add(new StatDisplay
            {
                Type = StatDisplayTypes.Morale,
                DisplayName = "Morale",
                CurrentValue = cm.MoraleAvg,
                RateOfChange = cm.MoraleRateAvg,
                MaxValue = -1,
                TimeUnit = TimeUnits.Hour,
                Tooltip = "The morale of the colony. This indicates the happiness of the colony."
            });

            //Power
            StatDisplays.Add(new StatDisplay
            {
                Type = StatDisplayTypes.Power,
                DisplayName = "Power",
                CurrentValue = cm.PowerInUse,
                RateOfChange = -1,
                MaxValue = cm.MaxPowerAvailable,
                TimeUnit = TimeUnits.Hour,
                Tooltip = "Power generated/consumed"
            });

            //Cash
            StatDisplays.Add(new StatDisplay
            {
                Type = StatDisplayTypes.Cash,
                DisplayName = "Ilaanbux",
                CurrentValue = cm.Cash,
                RateOfChange = cm.CashRate * 1000f * 60f * 60f,
                MaxValue = cm.MaxCash,
                TimeUnit = TimeUnits.Hour,
                Tooltip = "The money on the colony. Colonies generate more money with higher taxes."
            });

            //Tax rate
            StatDisplays.Add(new StatDisplay
            {
                Type = StatDisplayTypes.TaxRate,
                DisplayName = "Tax Rate",
                CurrentValue = cm.Sliders[SliderTypes.TaxRate].CurrentValue,
                RateOfChange = -1,
                MaxValue = 100,
                TimeUnit = TimeUnits.None,
                Tooltip = "Hard earned money stolen by big gubment fo dem programs"
            });

            Sliders = cm.Sliders.Values;
            WebStructuresOverviewData = new WebStructuresOverviewData(structureModels);
        }
    }
}
