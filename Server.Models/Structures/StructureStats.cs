using Freecon.Models.TypeEnums;

namespace Server.Models.Structures
{

    /// <summary>
    /// Class which stores static stats for different structures
    /// </summary>
    public class StructureStats
    {
        public virtual StructureTypes StructureType { get; set; }

        public float MaxHealth { get; set; }

        /// <summary>
        /// Units of thorium per milisecond.
        /// </summary>
        /// <value>
        /// The energy cost.
        /// </value>
        public float EnergyCost { get; set; }

        public int PeopleRequired { get; set; }
        public float PowerConsumptionRate { get; set; }//Amount of power consumed per update
        public float PopulationCost { get; set; }//Amount of population needed to run the building
        public WeaponTypes WeaponType { get; set; }

        public float StructureSizeX { get; set; }//In tiles
        public float StructureSizeY { get; set; }//In tiles

        public virtual StructureStats GetClone()
        {
            return (StructureStats)MemberwiseClone();
        }
    }

    public class TurretStats : StructureStats
    {
        public TurretStats()
        {
            StructureType = StructureTypes.LaserTurret;
            MaxHealth = 1000;
            EnergyCost = 0;
            PeopleRequired = 0;
            PowerConsumptionRate = 0;
            PopulationCost = 0;
            WeaponType = WeaponTypes.TurretLaser;
            StructureSizeX = 1;
            StructureSizeY = 1;
        }

        public override StructureStats GetClone()
        {
            return (TurretStats)MemberwiseClone();
        }
    }

    public class CommandCenterStats : StructureStats
    {
        public float PowerGenerationRate { get; set; }

        public override StructureTypes StructureType { get { return StructureTypes.CommandCenter; } }

        public CommandCenterStats()
        {
            PowerGenerationRate = 50;
            StructureSizeX = 2;
            StructureSizeY = 2;
        }

    }

    public class PowerPlantStats : StructureStats
    {
        public float MaxPowerGenerationRate { get; set; }

        public override StructureTypes StructureType { get { return StructureTypes.PowerPlant; } }

        public PowerPlantStats()
        {           
            MaxPowerGenerationRate = 200;
        }

    }

    public class BiodomeStats : StructureStats
    {
        public override StructureTypes StructureType { get { return StructureTypes.Biodome; } }

        public float MaxPopulation { get; set; }
        public float OrgRatePerPop { get; set; }
        public float MedRatePerPop { get; set; }
        public float HydRatePerPop { get; set; }
        public float PowerRatePerPop { get; set; }

        public float MaxMoraleRate { get; set; }//Per hour
        public float MoraleMultiplier { get; set; }//Multiplier determines the number of additional colonists based on morale, numExtra = MoraleMultiplier * Morale
        public float MaxMorale { get; set; }

        public float MaxPopChangeRate { get; set; }//Per hour


        public float OrgAndHydPopBonus {get;set;}
        public float OrgPopBonus { get; set; }
        public float HydPopBonus { get; set; }
        public float MedPopBonus { get; set; }
            
        public float MinResourcePopBonus { get; set; }//DO NOT LET THIS EQUAL 0, OTHERWISE POPULATION WILL TURN TO NaN

        public BiodomeStats()
        {
            //Resource consumption
            OrgRatePerPop = .1f;
            MedRatePerPop = .1f;
            HydRatePerPop = .1f;
            PowerRatePerPop = .01333f;//Set so that 15000 population consumes 200 power
            
           
            //Morale
            MoraleMultiplier = 10;
            MaxMorale = 1500;
            MaxMoraleRate = 20;

            //Resource bonuses - the idea is that targetPop = MaxPop * resourceBonus
            OrgAndHydPopBonus = 2f / 3f;
            OrgPopBonus = 1f / 5f;
            HydPopBonus = 1f / 5f;
            MedPopBonus = 1f / 3f;


            //Population
            MaxPopulation = 15000;
            MinResourcePopBonus = .0017f;//DO NOT LET THIS EQUAL 0, OTHERWISE POPULATION WILL TURN TO NaN
            MaxPopChangeRate = 700;
        }



    }

    /// <summary>
    /// Represents stats for the biodome which is automatically built when a colony is founded
    /// </summary>
    public class SmallBiodomeStats : BiodomeStats
    {
        public override StructureTypes StructureType { get { return StructureTypes.Biodome; } }

        public SmallBiodomeStats()
        {
            MaxPopulation = 2000;
        }

    }

    public class MineStats : StructureStats
    {
        public float BaseSupplyRate { get; set; }//Per hour
        public float BaseMineRate { get; set; }//Per hour

        public override StructureTypes StructureType { get{return StructureTypes.Mine;} }

        public MineStats()
        {
            BaseSupplyRate = 200;
            BaseMineRate = 100;

        }

    }

    public class DefensiveMineStats : StructureStats
    {

        public DefensiveMineStats()
        {
            //Intentionally small but non-zero to allow for overlapping mines in space; might have to snap to grid on planets
            StructureSizeX = .1f;
            StructureSizeY = .1f;
        }
    }

    public class FactoryStats : StructureStats
    {
        /// <summary>
        /// Construction points per ms
        /// </summary>
        public float BaseConstructionRate { get; set; }
        public float BaseMaxCargoSpace { get; set; }

        public FactoryStats()
        {
            BaseConstructionRate = 0.001f;
            BaseMaxCargoSpace = 10000;
        }

    }
}