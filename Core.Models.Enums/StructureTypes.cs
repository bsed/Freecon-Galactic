namespace Freecon.Models.TypeEnums
{
    public enum StructureTypes : byte
    {
        CommandCenter,
        Refinery,
        Mine,
        Silo,
        Factory,
        PowerPlant,
        LaserTurret,
        Biodome,
        ConstructionBuilding,
        DefensiveMine,
    }

    /// <summary>
    /// Distinguish between different types of turrets.
    /// </summary>
    public enum TurretTypes : byte
    {
        Space,
        Planet,
    }
}
