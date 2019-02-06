using Freecon.Core.Networking.Models.Objects;
using Freecon.Models;
using Freecon.Models.TypeEnums;
using Server.Models.Interfaces;
using Server.Models.Structures;
using SRServer.Services;
using System.Collections.Generic;

namespace Server.Models
{
    public class DefensiveMine : Structure<DefensiveMineModel, DefensiveMineStats>, ICanFire
    {
        public DebuffHandler Debuffs { get; protected set; }

        private IPlayerLocator _playerLocator;

        //Set to true to prevent sending multiple fire orders when multiple clients report hit
        public bool WasTriggered;
              
        public DefensiveMine(float xPos, float yPos, int galaxyID, int ownerID, int currentAreaID, IPlayerLocator playerLocator):base(xPos, yPos, galaxyID, ownerID, currentAreaID)
        {
            _model = new DefensiveMineModel(base._model);
            _model.StructureType = StructureTypes.DefensiveMine;
            Debuffs = new DebuffHandler();

            Id = galaxyID;

            _playerLocator = playerLocator;
            _model.Weapon = new MineWeapon();
        }

        public DefensiveMine(DefensiveMineModel model, IPlayerLocator playerLocator):base(model)
        {
            _model = model;
            _playerLocator = playerLocator;

        }
        public override StructureData GetNetworkData()
        {
            StructureData data = new StructureData();
            data.XPos = PosX;
            data.YPos = PosY;
            data.CurrentHealth = CurrentHealth;
            data.Id = Id;
            data.StructureType = StructureType;

        
            
            data.OwnerTeamIDs = _playerLocator.GetPlayerAsync(OwnerID).Result.GetTeamIDs();
            

            return data;
        }

        public HashSet<int> GetTeamIDs()
        {
            if (OwnerID == null)
                return new HashSet<int>();
            else
                return _playerLocator.GetPlayerAsync(OwnerID).Result.GetTeamIDs();
        }

        public Weapon GetWeapon(int slot)
        {
            return Weapon;
        }
    }

    public class DefensiveMineModel : StructureModel<DefensiveMineStats>
    {
        public Weapon GetWeapon(int slot)
        {
            return Weapon;
        }

        public DefensiveMineModel()
        {
            StructureType = StructureTypes.LaserTurret;
        }

        public DefensiveMineModel(StructureModel<DefensiveMineStats> s)
            : base(s)
        {
            
        }
    }


    public class DefensiveMineStats : StructureStats
    {
        public override StructureTypes StructureType { get{return StructureTypes.DefensiveMine;} }
        public float SplashRadius = 1;

        /// <summary>
        /// As of this writing, decay is linear with distance, goes from max to 0 from SplashRadius to SplashRadius + SplashDecayRadius 
        /// </summary>
        public float SplashDecayRadius = 1;

        
    }

}