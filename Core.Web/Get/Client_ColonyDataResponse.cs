using Server.Models;

namespace Core.Web.Get
{
    public class Client_ColonyDataResponse
    {
        public string Name { get; set; }

        public float TotalPopulation { get; set; }
        public float MaxPopulation { get; set; }
        public float MaxPowerAvailable { get; set; }

        public bool UseStoredThoriumForPower { get; set; }
        
        public float ThoriumOreStockpiled { get; set; }
        public float HydrogenStockpiled { get; set; }
        public float IronOreStockpiled { get; set; }
        public float OrganicsStockpiled { get; set; }
        public float HydrocarbonsStockpiled { get; set; }
        public float BauxiteStockpiled { get; set; }
        public float SilicaStockpiled { get; set; }

        public int SystemID { get; set; }


        public Client_ColonyDataResponse(ColonyModel model)
        {
            Name = model.Name;
        }
    }
}
