using Freecon.Models.TypeEnums;

namespace Freecon.Core.Networking.Models.Objects
{
    public class PlanetData_SystemView: OrbitingObject
    {
        public byte Mass;
        public byte Gravity;

        public byte Scale;
        public PlanetTypes PlanetType;
        public int IDToOrbit;
        public int Id;

    }
}