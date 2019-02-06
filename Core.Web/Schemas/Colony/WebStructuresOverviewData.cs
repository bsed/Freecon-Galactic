using System.Collections.Generic;
using Server.Models.Structures;

namespace Core.Web.Schemas
{
    public class WebStructuresOverviewData
    {
        public List<WebStructureOverviewData> Structures { get; set; }

        public List<WebConstructionData> ConstructionQueue { get; set; }

        public WebStructuresOverviewData(IEnumerable<IStructureModel> structureModels)
        {
            Structures = new List<WebStructureOverviewData>();
            ConstructionQueue = new List<WebConstructionData>();
            foreach(IStructureModel s in structureModels)
            {
                if(s is IConstructionStructureModel)
                {
                    ConstructionQueue.Add(new WebConstructionData((IConstructionStructureModel)s));
                }
                else
                {
                    Structures.Add(new WebStructureOverviewData(s));
                }


            }
        }
    }
}