namespace Freecon.Core.Models.Enums
{
    //The alternative to this is to combine stateful and stateless cargo into one enum, but that's awkward elsewhere, so we'll contain this mess in the port state
    //Currently, these must map to StatefulCargoTypes or StatelessCargoTypes as we use Enum.TryParse to convert from one to the other, for convenience.
    public enum PortWareIdentifier : byte
    {
        LaserTurret,
        DefensiveMine,
        Module,

        PlasmaCannon,
        Laser,
        EnergyDisruptor,

        Hull1,
        Hull2,
        Hull3,

        Engine1,
        Engine2,
        Engine3,

        Penguin,
        Reaper,
        Battlecruiser,
        Barge,

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

        AmbassadorMissile,
        HellHoundMissile,

        Biodome,
        CargoHold,

        HullRepair,
        ShieldRecharge,
        ShipChange,


        Null,



    }
}
