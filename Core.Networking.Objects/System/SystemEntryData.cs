using System.Collections.Generic;

namespace Freecon.Core.Networking.Models.Objects
{
    public class SystemEntryData : AreaEntryData
    {
        public StarData StarData;

        public List<PlanetData_SystemView> Planets;
        public List<MoonData_SystemView> Moons;
        public List<PortData_SystemView> Ports;

        public SystemEntryData(AreaEntryData areaData) : base(areaData)
        {
            Planets = new List<PlanetData_SystemView>();
            Moons = new List<MoonData_SystemView>();
            Ports = new List<PortData_SystemView>();

        }

        public SystemEntryData()
        {
            Planets = new List<PlanetData_SystemView>();
            Moons = new List<MoonData_SystemView>();
            Ports = new List<PortData_SystemView>();

        }        

    }
}