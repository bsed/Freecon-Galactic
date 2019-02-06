namespace Freecon.Models.TypeEnums
{
    public enum ProjectileTypes : byte
    {
        Laser,
        EnergyDisruptor,
        BC_Laser,
        LaserWave,
        NaniteLauncher,
        Orb,
        PlasmaCannon,
        GravityBomb,
        HullPlague,

        //Splash
        MissileSplash,
        MineSplash,

        //Missiles
        AmbassadorMissile,
        HellHoundMissile,
        MissileType1,//When these names are changed, be sure to also change names in StatelessCargoTypes to match, since we [unfortunately] make use of enum.TryParse for conversion...
        MissileType2,
        MissileType3,
        MissileType4,
    }

    public enum OriginTypes : byte
    {
        Local,//Fired by player
        Network,//Fired by network ship
        Turret,//Fired by turret
    }
}
