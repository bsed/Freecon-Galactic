using Server.Models.Structures;

namespace Core.Web.Schemas
{
    /// <summary>
    /// Structure data to display on the main page.
    /// </summary>
    public class WebStructureOverviewData
    {
        public string DisplayName { get; set; }

        public bool IsEnabled { get; set; }

        public float CurrentHealth { get; set; }

        public WebStructureInfo StructureInfo { get; set; }

        public WebStructureOverviewData(IStructureModel structureModel)
        {
            DisplayName = structureModel.StructureType.ToString();
            IsEnabled = structureModel.Enabled;
            CurrentHealth = structureModel.CurrentHealth;
            StructureInfo = new WebStructureInfo(structureModel);
        }
    }
}