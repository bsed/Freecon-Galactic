using System.Collections.Generic;
using Freecon.Models.TypeEnums;
using Server.Models;
using Server.Models.Structures;

namespace Core.Web.Schemas
{
    /// <summary>
    /// Served up as the full response to 
    /// </summary>
    public class ClientFullColonyStateDataResponse
    {
        public ColonyMetaData MetaData { get; set; }

        public ClientColonyPages Pages { get; set; } 

        public ClientFullColonyStateDataResponse(ColonyModel cm, PlanetTypes planetType, IEnumerable<IStructureModel> structureModels)
        {
            Pages = new ClientColonyPages() { Overview = new ColonyOverviewPageData(cm, structureModels) };
            MetaData = new ColonyMetaData() { PlanetType = planetType, ColonyName = cm.Name };
        }
    }
}
