using Freecon.Models.TypeEnums;
using System.Collections.Generic;

namespace DBFiller
{
    public class DBFillerConfig
    {
        public string UserdataTextFilepath = "../../../MockUserData.txt";
        
        public DBFillerPortConfig PortConfig = new DBFillerPortConfig();

        public int NumPlanetsPerSystem = 20;

        public int NumSystems = 20;

        public int NumNPCsPerSystem = 10;

        public int NumMinesPerSystem = 0;

        //Creates by going down the list in UserDataTextFilepath
        public int NumPlayers = 50;

        public int NumTurretsPerNPC = 0;


        //Ship Cargo
        public bool LeaveCargoEmpty = false;//Set this to true if you need to test cargo pickup
        public int CARGO_NumMines = 100;
        public int CARGO_NumTurrets = 100;
        public int CARGO_NumMissiles = 6666;
        public int CARGO_NumBiodomes = 100;        

        public int NumNpcsPerPlanet = 0;
        
    }

    public class DBFillerPortConfig
    {
 

        public Dictionary<StatefulCargoTypes, int> StatefulCargoCounts = new Dictionary<StatefulCargoTypes, int>();

        public Dictionary<StatelessCargoTypes, float> StatelessCargoCounts = new Dictionary<StatelessCargoTypes, float>();

        public Dictionary<ModuleTypes, int> ModuleCounts = new Dictionary<ModuleTypes, int>();

        public DBFillerPortConfig()
        {
            StatefulCargoCounts.Add(StatefulCargoTypes.LaserTurret, 500);
            StatefulCargoCounts.Add(StatefulCargoTypes.Barge, 5);
            StatefulCargoCounts.Add(StatefulCargoTypes.BattleCruiser, 5);
            StatefulCargoCounts.Add(StatefulCargoTypes.DefensiveMine, 500);
            StatefulCargoCounts.Add(StatefulCargoTypes.Penguin, 5);
            StatefulCargoCounts.Add(StatefulCargoTypes.Reaper, 5);
            StatefulCargoCounts.Add(StatefulCargoTypes.Laser, 500);            


            StatelessCargoCounts.Add(StatelessCargoTypes.AmbassadorMissile, 500);
            StatelessCargoCounts.Add(StatelessCargoTypes.Biodome, 500);
            StatelessCargoCounts.Add(StatelessCargoTypes.Hydrocarbons, 99999);
            StatelessCargoCounts.Add(StatelessCargoTypes.Organics, 99999);

            ModuleCounts.Add(ModuleTypes.ThrustModule, 10);
            ModuleCounts.Add(ModuleTypes.MaxShieldModule, 10);
            ModuleCounts.Add(ModuleTypes.TopSpeedModule, 10);

        }



    }
}
