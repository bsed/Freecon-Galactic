namespace Core.Models.Enums
{
    public enum ProblemFlagTypes:byte
    {
        //Warning
        LowOrganics,
        NegativeOrganicsRate,
        LowMedicine,
        NegativeMedicineRate,
        LowThorium,
        NegativeThoriumRate,
        LowHydrocarbons,
        NegativeHydrocarbonsRate,

        //Critical
        OrganicsDepleted,
        MedicineDepleted,
        ThoriumDepeleted,
        HydrocarbonsDepleted,
        UnderAttack,
        NotEnoughPower,
    
    }
}
