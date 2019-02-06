using System.Collections.Generic;

namespace Freecon.Core.Networking.Models.Objects
{
    public class PortEntryData: AreaEntryData
    {
        public List<PortShipData> Ships;

        public PortEntryData(AreaEntryData areaData) : base(areaData)
        {
            Ships = new List<PortShipData>();
        }

        public PortEntryData()
        {
            Ships = new List<PortShipData>();
        }

    }
}
