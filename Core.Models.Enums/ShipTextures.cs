namespace Core.Models.Enums
{
    /// <summary>
    /// Ships added here must have their texture assigned in the Dictionary at CustomShip.cs
    /// </summary>
    public enum ShipTextures : byte
    {
        Battlecruiser,
        Penguin, // OldJeyth
        ZYVariantBarge,
        Reaper,
        Mothership,
        // Below this line don't have textures yet
        JeythAssualt,
        JeythSupport,
        JeythFreigther,
        JeythTactical,
        NebulanCruiser,
        Pirani,
        SMFighter,
        Rasputin,
        Dread,

        None
    }
}