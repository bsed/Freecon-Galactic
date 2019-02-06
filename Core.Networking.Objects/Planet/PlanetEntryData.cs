using Freecon.Models.TypeEnums;

namespace Freecon.Core.Networking.Models.Objects
{
    public class PlanetEntryData : AreaEntryData
    {
      
        public PlanetTypes PlanetType;

        public int? PlanetTeamID { get; set; }
    
        public LayoutData Layout { get; set; }

        public PlanetEntryData()
        {
        }

        public PlanetEntryData(AreaEntryData a) : base(a)
        {
        }

    }

    public class LayoutData
    {
        public int NumX { get; set; }
        public int NumY { get; set; }

        public bool[] Layout1D { get; set; }

    }
}
