using System.Collections.Generic;
using Core.Web.Schemas;
using Core.Web.Schemas.Components;
using Freecon.Models.TypeEnums;
using Core.Models.Enums;
using Server.Models;
using Core.Models;
using Freecon.Models;
using Server.Models.Structures;

namespace Core.Web
{
    public class MockResponses
    {
        public ClientFullColonyStateDataResponse GetFakeColonyData()
        {
            var data = new ClientFullColonyStateDataResponse(new ColonyModel(), PlanetTypes.Frozen, new List<IStructureModel>());

            data.MetaData = new ColonyMetaData()
            {
                ColonyName = "Test Planet",
                PlanetType = PlanetTypes.Earthlike
            };
            data.Pages = new ClientColonyPages()
            {
                Overview = GetColonyOverviewPageData()
            };

            return data;
        }

        public ColonyOverviewPageData GetColonyOverviewPageData()
        {
            var sliders = new List<Slider>()
            {
                new Slider(SliderTypes.Construction, 30),
                new Slider(SliderTypes.Industry, 45),
                new Slider(SliderTypes.Research, 25),
            };

            var eventData = new List<ColonyNewsEvent>()
            {
                new ColonyNewsEvent()
                {
                    Color = WebColors.White.ToString(),
                    Message = "A space probe was found."
                },
                new ColonyNewsEvent()
                {
                    Color = WebColors.Red.ToString(),
                    Message = "A building was taken offline due to damage."
                }
            };

            var indicators = new List<StatusIndicatorData>()
            {
                new StatusIndicatorData(ProblemFlagTypes.NotEnoughPower),
                new StatusIndicatorData(ProblemFlagTypes.OrganicsDepleted),
            };

            var statDisplay = new List<StatDisplay>()
            {
                new StatDisplay()
                {
                    DisplayName = "Population",
                    CurrentValue = 1000,
                    RateOfChange = 5,
                    Type = StatDisplayTypes.Population,
                    TimeUnit = TimeUnits.Hour,
                    Tooltip = "The population of the colony. This indicates the size of the colony."
                },
                new StatDisplay()
                {
                    DisplayName = "Morale",
                    CurrentValue = 420,
                    RateOfChange = 11,
                    Type = StatDisplayTypes.Morale,
                    TimeUnit = TimeUnits.Hour,
                    Tooltip = "The morale of the colony. This indicates the happiness of the colony."
                },
                new StatDisplay()
                {
                    DisplayName = "Treasury",
                    CurrentValue = 1203240,
                    RateOfChange = 31202,
                    Type = StatDisplayTypes.Cash,
                    TimeUnit = TimeUnits.Hour,
                    Tooltip = "The money on the colony. Colonies generate more money with higher taxes."
                }
            };

            var overview = new ColonyOverviewPageData(new ColonyModel(), new List<IStructureModel>())
            {
                IsEnabled = true,
                Order = 0,
                ColonyEvents = eventData,
                PageName = "Overview",
                PageType = ColonyPages.Overview,
                Sliders = sliders,
                WebStructuresOverviewData = GetWebStructuresOverviewData(),
                StatusIndicators = indicators,
                StatDisplays = statDisplay
            };

            return overview;
        }
        

        public WebStructuresOverviewData GetWebStructuresOverviewData()
        {
            var data = new WebStructuresOverviewData(new List<IStructureModel>());

            data.ConstructionQueue = new List<WebConstructionData>()
            {
                
            };

            data.Structures = new List<WebStructureOverviewData>()
            {
                GenerateWebStructure(StructureTypes.Refinery),
                GenerateWebStructure(StructureTypes.Mine)
            };

            return data;
        }

        public WebStructureOverviewData GenerateWebStructure(StructureTypes type)
        {
            return null;
        }

        public WebStructureInfo GetFakeWebStructureInfo(StructureTypes type)
        {
            return null;
            //Fuck you don't judge me
        }
    }
}
