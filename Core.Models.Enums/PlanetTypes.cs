namespace Freecon.Models.TypeEnums
{
    public enum PlanetTypes : byte
    {
        // All normal planets must be 300px
        Ice = 0,
        Earthlike,
        Desert,
        DesertTwo,
        Rocky,
        Barren,
        Gray,
        Red,
        ColdGasGiant,
        IceGiant,
        HotGasGiant,
        Radioactive,
        OceanicLarge,
        Frozen,
        // Small planets only, 200px
        Crystalline,
        Gaia,
        OceanicSmall,
        // Special
        Paradise, // 300px
        Port
    }
}
