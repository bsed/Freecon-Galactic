using System;
using System.Collections.Generic;
using Lidgren.Network;
using Freecon.Models.TypeEnums;
using SRServer.Services;
using Server.Models.Interfaces;
using Server.Models.Extensions;
using Server.Interfaces;
using Core.Models;
using Core.Models.CargoHandlers;
using Core.Models.Enums;
using Freecon.Core.Interfaces;
using Freecon.Models;
using Freecon.Core.Networking.Models.Objects;
using Freecon.Core.Utils;
using Freecon.Models.UI;

namespace Server.Models
{

    public abstract class Ship<ShipModelType> : IShip
        where ShipModelType : ShipModel, new()
    {
        public int Id
        {
            get { return _model.Id; }
            set { _model.Id = value; }
        }

        public virtual string UIDisplayName
        {
            get { return ShipStats.ShipType.ToString().SplitCamelCase(); }
        }

        #region State Variables (position, energy, etc)

        public ShieldHandler Shields { get; protected set; }

        public float AngVel
        {
            get { return _model.AngVel; }
            set { _model.AngVel = value; }
        }

        public float CurrentEnergy
        {
            get { return _model.CurrentEnergy; }
            set { _model.CurrentEnergy = value; }
        }

        public float CurrentHealth
        {
            get { return _model.CurrentHealth; }
            set { _model.CurrentHealth = value; }
        }

        public bool IsDead { get; set; }
        public float KillTimeStamp { get; set; } //Set when killed
        public float RespawnTimeDelay { get; set; } //Set when killed

        public float PosX
        {
            get { return _model.PosX; }
            set { _model.PosX = value; }
        }

        public float PosY
        {
            get { return _model.PosY; }
            set { _model.PosY = value; }
        }

        public float Rotation
        {
            get { return _model.Rotation; }
            set { _model.Rotation = value; }
        }

        public bool Thrusting { get; set; }

        public float VelX
        {
            get { return _model.VelX; }
            set { _model.VelX = value; }
        }

        public float VelY
        {
            get { return _model.VelY; }
            set { _model.VelY = value; }
        }

        public float LastHealthUpdateTime { get; set; }
        public float HealthUpdatePeriod { get; set; }

        /// <summary>
        /// Area to send ship to on revival
        /// </summary>
        public int? ReviveAreaID { get; set; }

        [UIProperty]
        public float MaxHealth
        {
            get { return ShipStats.MaxHealth + StatBonuses[StatBonusTypes.MaxHealth]; }
        }

        [UIProperty(Units = "TW")]
        public float MaxShields
        {
            get { return ShipStats.MaxShields + StatBonuses[StatBonusTypes.MaxShields]; }
        }

        [UIProperty(Units = "TW")]
        public float MaxEnergy
        {
            get { return ShipStats.Energy + StatBonuses[StatBonusTypes.MaxEnergy]; }
        }

        #endregion

        public ShipBonusHandler StatBonuses { get; protected set; }

        public DebuffHandler Debuffs { get; protected set; }

        protected ShipModelType _model;

        public bool IsNPC
        {
            get { return _model.IsNPC; }
            set { _model.IsNPC = value; }
        }


        #region Efficiency Variables

        protected float combatUpdateTimeout = 40000; //milliseconds
        public bool DoCombatUpdates { get; set; } //Set to true when the IShip collides with a projectile.

        public float TimeOfLastCollision { get; set; }

        #endregion

        protected IPlayerLocator _playerLocator;
        protected IAreaLocator _areaLocator;
        protected ITeamLocator _teamLocator;

        #region References to Other Objects (E.G. CurrentArea)

        public int? CurrentAreaId
        {
            get { return _model.CurrentAreaID; }
        }


        public int? SimulatingPlayerID
        {
            get { return _simulatingPlayerID; }
        }

        protected int? _simulatingPlayerID;

        public int? PlayerID
        {
            get { return _model.PlayerID; }
        }

        #endregion

        public float lastTimeStamp = 0;

        List<Weapon> _weapons
        {
            get { return _model.Weapons; }
            set { _model.Weapons = value; }
        }

        //Each ship gets one missile launcher? Sound reasonable? Sure, why not?
        public MissileLauncher MissileLauncher
        {
            get { return (MissileLauncher) _weapons[_model.MissileLauncherSlot]; }
        }

        
        public CargoHandler_ReadOnlyVM<CargoHandlerModel> Cargo { get; private set; }

        [UICollection]
        public ShipStats ShipStats { get { return _model.ShipStats; } set { _model.ShipStats = value; Shields.ShipStats = value; } }
        
        public PilotTypes PilotType { get{return _model.PilotType;} protected set{_model.PilotType = value;} }

        public float TimeOfLastDamage { get; set; }

        public float LastWarpTime { get; set; }

        protected Ship() { }

        public Ship(ShipStats stats, LocatorService ls)
        {
            _model = new ShipModelType();
            PosX = 0;
            PosY = 0;
            Rotation = 0;
            VelY = 0;
            VelX = 0;
            _weapons = new List<Weapon>();
            _model.MissileLauncherSlot = 0;
            _weapons.Add(new MissileLauncher(ProjectileTypes.AmbassadorMissile));

            
            
            TimeOfLastCollision = 0;
            TimeOfLastDamage = 0;
            DoCombatUpdates = false;

            HealthUpdatePeriod = 500;

            _model.PlayerID = null;
            _simulatingPlayerID = null;
            IsNPC = false;


            _playerLocator = ls.PlayerLocator;
            _areaLocator = ls.AreaLocator;
            _teamLocator = ls.TeamLocator;
                               

            _model.Modules = new List<Module>();
            StatBonuses = new ShipBonusHandler();

            Debuffs = new DebuffHandler();

            switch (stats.ShieldType)
            {
                case ShieldTypes.QuickRegen:
                    Shields = new QuickRegenShieldHandler(stats, StatBonuses, Debuffs);
                    break;

                case ShieldTypes.SlowRegen:
                    Shields = new SlowRegenShieldHandler(stats, StatBonuses, Debuffs);
                    break;

                case ShieldTypes.NoRegen:
                    Shields = new NoRegenShieldHandler(stats, StatBonuses, Debuffs);
                    break;
            }
            ShipStats = stats;
            
            
            _model.Cargo = new CargoHandlerModel();
            Cargo = new CargoHandler_ReadOnlyVM<CargoHandlerModel>(_model.Cargo);
            SetHealthAndShields(stats);
        }

        public Ship(ShipModelType s, LocatorService ls)
        {
            _model = s;

            StatBonuses = new ShipBonusHandler();

            Debuffs = new DebuffHandler();

            switch (ShipStats.ShieldType)
            {
                case ShieldTypes.QuickRegen:
                    Shields = new QuickRegenShieldHandler(ShipStats, StatBonuses, Debuffs);
                    break;

                case ShieldTypes.SlowRegen:
                    Shields = new SlowRegenShieldHandler(ShipStats, StatBonuses, Debuffs);
                    break;

                case ShieldTypes.NoRegen:
                    Shields = new NoRegenShieldHandler(ShipStats, StatBonuses, Debuffs);
                    break;
            }

            RecalculateModuleBonuses();

            Cargo = new CargoHandler_ReadOnlyVM<CargoHandlerModel>(s.Cargo);
            
            
            _playerLocator = ls.PlayerLocator;
            _areaLocator = ls.AreaLocator;
            _teamLocator = ls.TeamLocator;

        }

        public void ChangeShipType(ShipStats shs)
        {
            ShipStats = shs;
            SetHealthAndShields(shs);
        }
                
        public void ChangeEnergy(float amount)
        {
            if (CurrentEnergy + amount > MaxEnergy)
                CurrentEnergy = MaxEnergy;
            else if (CurrentEnergy + amount < 0)
                CurrentEnergy = 0;
            else
                CurrentEnergy += amount;
        }

        public virtual void Update()
        {
            ChangeEnergy((ShipStats.EnergyRegenRate * StatBonuses[StatBonusTypes.EnergyRegen] / (1 + Debuffs[DebuffTypes.EnergyRegen] * DebuffHandlerModel.EffectValues[DebuffTypes.EnergyRegen])) * (TimeKeeper.MsSinceInitialization - lastTimeStamp));
                //Energy rate given in energy/millisecond
                
            foreach (var w in _weapons)
            {
                w.Update();
            }

            if (DoCombatUpdates)
            {
                Debuffs.Update(TimeKeeper.MsSinceInitialization);

                Shields.Update(TimeKeeper.MsSinceInitialization);
                if (combatUpdateTimeout + TimeOfLastCollision < TimeKeeper.MsSinceInitialization)
                    DoCombatUpdates = false;
            }
            lastTimeStamp = TimeKeeper.MsSinceInitialization;
        }

        #region Health, Shields, and Damage

        /// <summary>
        /// Sends damage amount to client
        /// Updates IShip health/shields appropriately
        /// Also handles projectile effects, if applicable
        /// </summary>
        /// <param name="projectileType"></param>
        /// <param name="projectileID"></param>
        /// <param name="pctCharge"></param>
        /// <param name="galaxyManager"></param>
        /// <returns>Returns true if the IShip is killed, false otherwise</returns>    
        public bool TakeDamage(ProjectileTypes projectileType, byte pctCharge, float multiplier)
        {
            float damageAmount = this.DamageAmount(projectileType, pctCharge) / StatBonuses[StatBonusTypes.Defense] * (1 + Debuffs[DebuffTypes.Defense]);
            float energyAmount = this.EnergyAmount(projectileType, pctCharge) / StatBonuses[StatBonusTypes.Defense] * (1 + Debuffs[DebuffTypes.Defense]);

            ChangeEnergy(-energyAmount);
                       
            TimeOfLastDamage = TimeKeeper.MsSinceInitialization;


            if (!IsDead)
            {
                if (Shields.CurrentShields == 0) //If shields are already at 0, player takes damage
                    CurrentHealth -= damageAmount;
                else if (Shields.CurrentShields - damageAmount < 0) //If there are not enough shields to take the damage
                    Shields.CurrentShields = 0;
                else
                    Shields.CurrentShields -= damageAmount;
                
            }


            if (CurrentHealth <= 0 && !IsDead)
                return true;            
            else //If IShip is not dead
                return false;

            
        }        
        
        private void SetHealthAndShields(ShipStats shs)
        {
            CurrentHealth = shs.MaxHealth + StatBonuses[StatBonusTypes.MaxHealth];
            Shields.CurrentShields = shs.MaxShields + StatBonuses[StatBonusTypes.MaxShields];
            CurrentEnergy = 0;
        }

        #endregion       
               
        public virtual HashSet<int> GetTeamIDs()
        {
            if (_model.PlayerID == null)
                return null;
            else
                return GetPlayer().GetTeamIDs();

        }

        #region Reference Getters and Setters

        public IArea GetArea()
        {
            if (_areaLocator == null)
                throw new Exception("Error: Ship.GetArea() called on IShip with ID " + Id.ToString() + " but _areaLocator service reference is null.");

            return _areaLocator.GetArea(CurrentAreaId);

        }

        public IShip SetArea(IArea newArea)
        {
            if (newArea != null)
                _model.CurrentAreaID = newArea.Id;
            else
                _model.CurrentAreaID = null;

            return this;
        }

        /// <summary>
        /// Returns the ship's current player, or null if no player is assigned.
        /// </summary>
        /// <returns></returns>
        public Player GetPlayer()
        {
            if (_playerLocator == null)
                throw new Exception("Error: Ship.GetPlayer() called on IShip with ID " + Id.ToString() + " but _playerLocator service reference is null.");

            if (_model.PlayerID != null)
                return _playerLocator.GetPlayerAsync((int)_model.PlayerID).Result;
            else
                return null;
        }

        public IShip SetPlayer(Player p)
        {
            if (p != null)
            {
                _model.PlayerID = p.Id;
                _model.IsNPC = p.PlayerType == PlayerTypes.NPC;
                _simulatingPlayerID = p.Id;
            }
            else
            {
                _model.PlayerID = null;
                _model.IsNPC = false;
                _simulatingPlayerID = null;
            }
            return this;
        }

        public Player GetSimulatingPlayer()
        {
            if (_playerLocator == null)
                throw new Exception("Error: Ship.GetSimulatingPlayer() called on IShip with ID " + Id.ToString() + " but _playerLocator service reference is null.");

            return _playerLocator.GetPlayerAsync(_simulatingPlayerID).Result;
        }

        #endregion        

        #region Writing Stats to Messages

        /// <summary>
        /// Writes the ships stats and type to the provided message     
        /// </summary>
        /// <param name="msg"></param>
        public virtual ShipData GetNetworkData(bool writeTeams, bool writeMods, bool writeCargo, bool writeStats = false)
        {
            ShipData data = new ShipData();
            data.ShipType = ShipStats.ShipType;
            data.IsNPC = IsNPC;
            data.PlayerID = PlayerID;
            data.Id = Id;
            data.PosX = PosX;
            data.PosY = PosY;
            data.VelX = VelX;
            data.VelY = VelY;
            data.Rotation = Rotation;
            Player p = GetPlayer();
            data.PlayerName = p == null ? null : p.Username;
            data.ShieldType = ShipStats.ShieldType;
            data.CurrentHealth = CurrentHealth;
            data.CurrentShields = Shields.CurrentShields;
            data.IsDead = IsDead;
            data.PilotType = PilotType;
            data.SelectedMissileType = MissileLauncher.Stats.ProjectileType;
            
            foreach(Weapon w in _weapons)
            {
                data.WeaponTypes.Add(w.Stats.WeaponType);
            }
            
            if (writeStats)
            {
                data.ShipStats = ShipStats.GetNetworkData();
            }            

            if (writeTeams)
            {               
                WriteTeamStateToMessage(data);
            }

            if (writeCargo)
            {
                Cargo.WriteStatsToMessage(data);
            }
            else if(writeMods)
            {
                if(data.Cargo == null)
                {
                    data.Cargo = new CargoData();
                }
                foreach(var m in _model.Modules)
                {
                    data.Cargo.StatefulCargo.Add(m.GetNetworkObject());
                }
            }

            return data;

        }

        public virtual void WriteTeamStateToMessage(ShipData data)
        {
            var player = GetPlayer();
            var IDs = player?.GetTeamIDs();
            if (IDs == null)
                return;

            foreach (int i in IDs)
            {
                data.TeamIDs.Add(i);
            }
        }
        
        #endregion

        #region Module Stuff
        public void CargoAdded(object sender, ITransactionAddStatefulCargo transaction)
        {
            if(transaction.CargoType == StatefulCargoTypes.Module)
            {
                AddModule((Module)transaction.CargoObject);
            }
        }

        public void CargoRemoved(object sender, ITransactionRemoveStatefulCargo transaction)
        {
            if (transaction.CargoType == StatefulCargoTypes.Module)
            {
                RemoveModule((Module)transaction.RemovedCargo);
            }
        }

        /// <summary>
        /// Function properly adds a module, recalculating stat bonuses appropriately
        /// </summary>
        /// <param name="m"></param>
        void AddModule(Module m)
        {
            _model.Modules.Add(m);
            StatBonuses.AddBonus(m);
        }

        void RecalculateModuleBonuses()
        {
            StatBonuses.Reset();

            foreach (Module m in _model.Modules)
                StatBonuses.AddBonus(m);
        }
        void RemoveModule(Module m)
        {
            _model.Modules.Remove(m);
            StatBonuses.RemoveBonus(m);
        }

        public List<Module> GetModules()
        {
            return new List<Module>(_model.Modules);
        }

        /// <summary>
        /// Gets the module IDs. Use the IDs to get/remove modules from cargo using CargoSynchronizer
        /// </summary>
        /// <returns></returns>
        public List<int> GetModuleIDs()
        {
            List<int> retList = new List<int>();
            foreach(var m in _model.Modules)
            {
                retList.Add(m.Id);
            }
            return retList;
        }

        #endregion

        /// <summary>
        /// if slot==-1, weapon is appended to the end of the weapon list
        /// </summary>
        /// <param name="w"></param>
        /// <param name="slot"></param>
        public void SetWeapon(Weapon w, int slot = -1)
        {
            if (slot == -1)
                slot = _weapons.Count;

            if (w.Stats.WeaponType == WeaponTypes.MissileLauncher)
            {
                throw new OhShitWhatTheFuckAreYouDoingYouAreNotGoodWithComputers("Missile launchers can't be assigned to ships, all ships have a default launcher in slot 0.");
            }


            while (_weapons.Count <= slot)
                _weapons.Add(new NullWeapon());//This is kind of ugly, but should be OK for now. Can't reference WeaponManager from this assembly

            _weapons[slot] = w;

        }

        public Weapon GetWeapon(int slot)
        {
            if (_weapons.Count > slot)
                return _weapons[slot];
            else
                return null;
        }

        public CargoHandler_ReadOnlyVM<CargoHandlerModel> GetCargo()
        {
            return Cargo;
        }

        public virtual IDBObject GetDBObject()
        {
            return _model.GetClone();    
        
        }





    }

    
    

}