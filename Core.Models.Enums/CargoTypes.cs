namespace Freecon.Models.TypeEnums
{
    //Cargo which has no state, and is represented only by count
    public enum StatelessCargoTypes : byte
    {
        #region Resources and Goods
        Cash,
        Hydrogen,
        Iron,
        Neutronium,
        Hydrocarbons,
        Woman,
        ThoriumOre,
        Thorium,
        IronOre,
        Organics,
        Bauxite,
        Silica,
        Water,
        Oxygen,
        Opium,
        #endregion

        AmbassadorMissile,
        HellHoundMissile,
        MissileType1,//When these names are changed, be sure to also change names in ProjectileTypes to match, since we [unfortunately] make use of enum.TryParse for conversion...
        MissileType2,
        MissileType3,
        MissileType4,
        MissileType5,

        Biodome,
        CargoHold,
        




        Null
    }

    /// <summary>
    /// Cargo with unique state for each item
    /// </summary>
    public enum StatefulCargoTypes : byte
    {
        LaserTurret,
        DefensiveMine,
        Module,

        PlasmaCannon,
        Laser,
        LaserWave,
        EnergyDisruptor,

        Hull1,
        Hull2,
        Hull3,

        Engine1,
        Engine2,
        Engine3,


        //Ships
        //Not sure if we'll actually allow ships to carry other ships, but this simplifies port buying/selling of ships
        Penguin,
        Reaper,
        BattleCruiser,
        Barge,



        Null,//Simplifies CheckStatefulCargo test

    }
}
