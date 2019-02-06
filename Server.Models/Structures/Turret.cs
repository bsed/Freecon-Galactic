using Freecon.Models.TypeEnums;
using Server.Managers;
using Server.Models.Extensions;
using Server.Models.Interfaces;
using SRServer.Services;
using Freecon.Models;
using Freecon.Core.Networking.Models.Objects;
using System.Collections.Generic;

namespace Server.Models.Structures
{
    public class Turret : Structure<TurretModel, TurretStats>, ICollidable, ICanFire, IKillable
    {
  
        public bool IsOnPlanet { get { return _model.IsOnPlanet; } set { _model.IsOnPlanet = value; } }//Temporary

        public TurretTypes TurretType { get { return _model.TurretType; } set { _model.TurretType = value; } }

        public float TimeOfLastCollision { get { return _model.TimeOfLastCollision; } set { _model.TimeOfLastCollision = value; } }
        public float TimeOfLastDamage { get { return _model.TimeOfLastDamage; } set { _model.TimeOfLastDamage = value; } }
        public bool DoCombatUpdates { get { return _model.DoCombatUpdates; } set { _model.DoCombatUpdates = value; } }

        IPlayerLocator _playerLocator;//This is a problem that we're going to have to deal with very soon; that of maintaining integrity of data distributed among servers, e.g. this turret's teams must be in sync with the player at all times...

        public DebuffHandler Debuffs { get; protected set; }

        public Turret()
        { }

        public Turret(int galaxyID, float xPos, float yPos, int ownerID, int currentAreaID, TurretTypes turretType, IPlayerLocator pl)
            : base(xPos, yPos, galaxyID, ownerID, currentAreaID)
        {
            _model = new TurretModel(base._model, turretType);
            _model.StructureType = StructureTypes.LaserTurret;
            Debuffs = new DebuffHandler();

            Id = galaxyID;

            _playerLocator = pl;
            _model.Weapon = new AltLaser();
            
        }

        public Turret(TurretModel tm, IPlayerLocator pl):base(tm)
        {
            _playerLocator = pl;
            Debuffs = new DebuffHandler();
        }
        
        public bool TakeDamage(ProjectileTypes projectileType, byte pctCharge, float multiplier)
        {
            float damageAmount = this.DamageAmount(projectileType, pctCharge);
            _model.CurrentHealth -= damageAmount;


            if (_model.CurrentHealth <= 0)
            {
#if DEBUG
                if (_model.CurrentHealth < 0)
                {
                    ConsoleManager.WriteLine("Turret health is " + _model.CurrentHealth, ConsoleMessageType.Debug);
                }
#endif
                
                _model.CurrentHealth = 0;
                return true;
            }
            return false;
        }

        public Weapon GetWeapon(int slot)
        {
            return Weapon;
        }

        public HashSet<int> GetTeamIDs()
        {
            if (OwnerID == null)
                return new HashSet<int>();
            else
                return _playerLocator.GetPlayerAsync(OwnerID).Result.GetTeamIDs();
        }

        public override StructureData GetNetworkData()
        {
            TurretData data = new TurretData();
            data.XPos = PosX;
            data.YPos = PosY;
            data.CurrentHealth = CurrentHealth;
            data.Id = Id;
            data.TurretType = TurretType;
            data.StructureType = StructureType;
            
            if(!IsOnPlanet)
            {
                data.OwnerTeamIDs = _playerLocator.GetPlayerAsync(OwnerID).Result.GetTeamIDs();
            }

            return data;
        }
    }


    public class TurretModel : StructureModel<TurretStats>
    {
        public float TimeOfLastCollision { get; set; }
        public bool DoCombatUpdates { get; set; }
        public float TimeOfLastDamage { get; set; }
        public bool IsOnPlanet { get;  set; }

        public TurretTypes TurretType {get; set;}
        
        public Weapon GetWeapon(int slot)
        {
            return Weapon;
        }

        public TurretModel()
        {
            StructureType = StructureTypes.LaserTurret;
        }
    
        public TurretModel(StructureModel<TurretStats> s, TurretTypes turretType):base(s)
        {
            TurretType = turretType;
        }


    }
}
