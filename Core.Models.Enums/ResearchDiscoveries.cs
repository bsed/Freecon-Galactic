namespace Core.Models.Enums
{
    public enum ResearchDiscoveries : byte
    {
        //Geoscience
        //Researches which reveal locations of resources?
        PlateTectonics,
        PlanetaryStructure,
        PlanetaryComposition,
        SoilCharacterization,
        Hydrogeology,
        PlanetaryPowerSources,//Power sources to be exploiated, unique to the planet/colony location

        //EnvironmentalEngineering
        SiteDesign,//Optimized design of staging areas for construction/mining
        EnvironmentalResistance,//Resistance of structures to the environment, bonus to construction rates and production rates (more uptime?)
        DefensiveEmplacement, //Bonus to defensive structure health
        MicroClimateControl, //Bonus to structure health?
        MineDesign, //Improved production rates
        StructuralOptimization,//Converts the colony to adapt to planetary conditions

        //ColonialEconomics
        //Mostly research which improves revenue from tax
        TaxDistribution, //Ideal tax rate per income
        CentralPlanning, //Socialist leaning market
        FreeMarkets, //Capitalistic leaning market
        EconomicPropaganda, //Consume or be shamed!

        //ColonialPsychiatry
        //Boosts to morale/reproduction
        PsychiatricEpidemiology,
        PsychologicalProfiling,
        Propaganda,//Should include a slider to adjust propaganda level, which can provide bonus to all aspects of the colony, with some probability of revolt which increases with slider level
        RedLightDistrict,// 8===D (oYo) ;)




    }
}