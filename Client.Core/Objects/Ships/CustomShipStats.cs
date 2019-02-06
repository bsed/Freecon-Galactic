using Core.Models.Enums;

namespace Freecon.Client.Core.Objectss
{
    public struct CustomShipStats
    {
        public string Name { get; set; }

        // ints
        public int Price { get; set; }
        public int Shields { get; set; }
        public int Hull { get; set; }
        public int Energy { get; set; }
        public int Cargo { get; set; }
        public int TopSpeed { get; set; }
        public int Acceleration { get; set; }

        // strings
        public ShipTextures Graphic { get; set; }
        public string ThrustGraphic { get; set; }
        public string Class { get; set; }

        // floats
        public float TurnRate { get; set; }
        public float RegenRate { get; set; }
    }
}
