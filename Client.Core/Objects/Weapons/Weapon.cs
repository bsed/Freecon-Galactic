using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Core.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using Freecon.Client.Interfaces;
using Freecon.Client.Managers;
using Freecon.Models;
using Core.Models.Enums;
using Freecon.Client.Core.Utils;
using Freecon.Models.TypeEnums;

namespace Freecon.Client.Objects.Weapons
{
    public abstract class Weapon
    {
        public WeaponStats Stats { get; protected set; }

        public WeaponTypes WeaponType { get { return Stats.WeaponType; } }

        protected ProjectileManager _projectileManager;

        /// <summary>
        /// Reference to object which holds the weapon to avoid passing.
        /// </summary>
        public ICanFire HoldingObj;

        /// <summary>
        /// The time of wait startTo make sure the ship doesn't get 
        /// Stuck perpetually unable to fire in case of a dropped message.
        /// </summary>
        public double TimeOfWaitStart;

        public float timeSinceLastShot;

        /// <summary>
        /// Tracks whether the client is holding down the fire key for a chargeable weapon
        /// </summary>
        public bool IsBeingHeld;

        public bool WaitingForFireResponse = false;
        protected float waitTimeOut = 1000;//ms

        public byte Slot;//Slot in the holding object's weapons list

        //Mostly a proof of concept for now. Can change to a list later for a weapon to have multiple debuffs
        public DebuffTypes DebuffType { get; set; }
      
        public Weapon(ProjectileManager projectileManager, byte slot, ICanFire holdingObj)
        {

            DebuffType = DebuffTypes.None;
            Slot = slot;
            HoldingObj = holdingObj;
            _projectileManager = projectileManager;
            timeSinceLastShot = float.MaxValue;
        }

        public virtual bool CanFire()
        {
            return HoldingObj.GetCurrentEnergy() >= Stats.EnergyCost &&//Check if there is enough energy 
                   timeSinceLastShot >= Stats.FirePeriod &&//Fire period check 
                   (!WaitingForFireResponse || TimeOfWaitStart > waitTimeOut);//Avoid spamming server with requests
        }

        
        /// <summary>
        /// Returns true succesful, false otherwise
        /// </summary>
        /// <param name="changeEnergy">If true, energy is deducted from the firing object</param>
        /// <returns></returns>
        protected virtual bool Fire(bool changeEnergy)
        {
            if (timeSinceLastShot < Stats.FirePeriod)
                return false;


            timeSinceLastShot = 0;
            WaitingForFireResponse = false;

            if (changeEnergy && HoldingObj != null)
                HoldingObj.ChangeEnergy((int)-Stats.EnergyCost);
                      
            return true;
        }

        
        /// <summary>
        /// Fires the weapon. Used for weapons which are fired by this client. Sends fire request to server. Generates a list of projectileIDs to be sent to the server.
        /// </summary>
        /// <param name="projectileIDs"></param>
        /// <param name="rotation"></param>
        /// <param name="charge"></param>
        public virtual bool Fire_LocalOrigin(float rotation, byte charge, bool changeEnergy)
        {
            if (!Fire(changeEnergy))
                return false;


            var projectileIDs = new List<int>(Stats.NumProjectiles);
           
            for (int i = 0; i < Stats.NumProjectiles; i++)
            {
                projectileIDs.Add(Utilities.NextUnique());//Ensures that all IDs are unique                   
  
            }

            List<Vector2> projectilePositions = GeneratePositions(rotation);
            List<float> projectileRotations = GenerateRotations(rotation);
            List<Vector2> projectileVelocities = GenerateVelocityOffsets(rotation);

            ProjectileRequest pr = new ProjectileRequest(HoldingObj, rotation, Slot, charge, true, Stats.ProjectileType);
            
            for(int i = 0; i < projectileIDs.Count; i++)
            {
                ProjectileData d = new ProjectileData(projectilePositions[i], projectileVelocities[i], projectileRotations[i], projectileIDs[i]);
                pr.AddProjectile(d);
            }



                 _projectileManager.CreateProjectiles(pr);

            return true;

        }

        /// <summary>
        /// Fires the weapon. Used for weapons which are fired by other clients, with data received from the server.
        /// </summary>
        /// <param name="projectileIDs"></param>
        /// <param name="rotation"></param>
        /// <param name="charge"></param>
        /// <returns></returns>
        public virtual bool Fire_ServerOrigin(List<int> projectileIDs, float rotation, byte charge, bool changeEnergy)
        {
            if (!Fire(changeEnergy))
                return false;

            List<Vector2> projectilePositions = GeneratePositions(rotation);
            List<float> projectileRotations = GenerateRotations(rotation);
            List<Vector2> projectileVelocities = GenerateVelocityOffsets(rotation);

            ProjectileRequest pr = new ProjectileRequest(HoldingObj, rotation, Slot, charge, false, Stats.ProjectileType);

            for (int i = 0; i < projectileIDs.Count; i++)
            {
                ProjectileData d = new ProjectileData(projectilePositions[i], projectileVelocities[i], projectileRotations[i], projectileIDs[i]);
                pr.AddProjectile(d);
            }

            _projectileManager.CreateProjectiles(pr);

            return true;
        }


        public virtual void Update(IGameTimeService gameTime)
        {
            timeSinceLastShot += (float)gameTime.ElapsedMilliseconds;

        }

        //TODO: Figure out if we want to generate and send projectile data (rotation, position, etc) on the originating client, or have each client generate projectiles based only on position and rotation.
        //Results in different combat feel under noticible latency. 

        protected abstract List<Vector2> GeneratePositions(float rotation);

        protected virtual List<float> GenerateRotations(float rotation)
        {
            var retlist = new List<float>();
            for(int i = 0; i < Stats.NumProjectiles; i++)
            {
                retlist.Add(rotation);
            }
            return retlist;
        }

        /// <summary>
        /// Velocity offset to be applied to initial projectile velocity (initial velocity based on projectile speed and rotation)
        /// defaults to holdingObj.LinearVelocity, to offset projectile velocities by ship velocity
        /// </summary>
        /// <param name="rotation"></param>
        /// <returns></returns>
        protected virtual List<Vector2> GenerateVelocityOffsets(float rotation)
        {
            var retlist = new List<Vector2>();
            for(int i = 0; i < Stats.NumProjectiles; i++)
            {
                retlist.Add(HoldingObj.LinearVelocity);
            }
            return retlist;
        }


        /// <summary>
        /// Shifts a bullet to a new position.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="u">Amount to offset the projectile by in the direction perpindicular to the rotation vector (i.e. "left or right" of projectile flight direction) direction</param>
        /// <param name="v">Amount to offset the projectile by in the direction parallel to the rotation vector (i.e. "up or down" in projectile flight direction) direction</param>
        /// <returns></returns>
        protected Vector2 OffsetBullet(Vector2 position, float rotation, float u, float v)
        {
            return new Vector2(position.X + (float)Math.Cos(rotation) * u - (float)Math.Cos(rotation + Math.PI / 2) * v,
                                 position.Y + (float)Math.Sin(rotation) * u - (float)Math.Sin(rotation + Math.PI / 2) * v);
        }

                
    }

    public abstract class ChargableWeapon : Weapon
    {
        public ChargableWeapon(ProjectileManager projectileManager, byte slot, ICanFire holdingObj) : base(projectileManager, slot, holdingObj) { }
       

        protected double _maxCharge = 50;
        protected double _chargeRate = 30;//Charge per s
        protected double _currentCharge = 0;
        protected double _energyPerCharge = 20;//Can take up to _energyPerCharge * _maxCharge energy during a charge

        protected byte _firePctCharge = 0;//Set on firing, since the projectile isn't fired until server response is received


        protected Texture2D _chargeTex;
        public Texture2D ChargeTex { get { return _chargeTex; } }

        public virtual float CurrentCharge { get { return (float)_currentCharge; } private set { _currentCharge = value; } }

        public virtual byte CurrentPctCharge { get { return (byte)(_currentCharge / _maxCharge); } }
       
        public override bool CanFire()
        {
            //Note: chargable weapons don't check for minium energy requirements
            return timeSinceLastShot >= Stats.FirePeriod &&//Fire period check 
                   (!WaitingForFireResponse || TimeOfWaitStart > waitTimeOut);//Avoid spamming server with requests
        }

    }

    
    
    /// <summary>
    /// Same as regular laser, except laser alternates between left and right firing
    /// </summary>
    public class AltLaser : Laser
    {
        private bool leftOrRight;//used to switch between left and right firing

        public AltLaser(ProjectileManager projectileManager, ICanFire holdingObj, byte slot)
            : base(projectileManager, holdingObj, slot)
        {
            Stats = new AltLaserStats();
        }              
       

        protected override List<Vector2> GeneratePositions(float rotation)
        {
            List<Vector2> positions = new List<Vector2>();
            

            if (leftOrRight)
            {
                positions.Add(OffsetBullet(HoldingObj.Position, rotation, .2f, 0));
                leftOrRight = false;
            }
            else
            {
                positions.Add(OffsetBullet(HoldingObj.Position, rotation, -.2f, 0));
                leftOrRight = true;
            }

            return positions;
        }
        
    }


    public class TurretAltLaser : AltLaser
    {
        private bool leftOrRight;//used to switch between left and right firing

        public TurretAltLaser(ProjectileManager projectileManager, ICanFire holdingObj, byte slot)
            : base(projectileManager, holdingObj, slot)
        {
            Stats = new TurretAltLaserStats();
        }

    }

    public class NullWeapon : Weapon
    {
        public NullWeapon(ICanFire holdingObject)
            : base(null, 0, holdingObject)
        {
            Stats = new NoneWeaponStats();
        }

 
        public override bool Fire_LocalOrigin(float rotation, byte charge, bool changeEnergy)
        {
            return false;
        }

        public override bool Fire_ServerOrigin(List<int> projectileIDs, float rotation, byte charge, bool changeEnergy)
        {
            return false;
        }

        protected override List<Vector2> GeneratePositions(float rotation)
        {
            return new List<Vector2>();
        }
    }


    public class MineWeapon : Weapon
    {
        public MineWeapon(ProjectileManager projectileManager, ICanFire holdingObj, byte slot)
            : base(projectileManager, slot, holdingObj)
        {
            Stats = new MineWeaponStats();
        }

        protected override List<Vector2> GeneratePositions(float rotation)
        {
            var retlist = new List<Vector2>{new Vector2(HoldingObj.Position.X, HoldingObj.Position.Y)};
            return retlist;
        }

        protected override List<Vector2> GenerateVelocityOffsets(float rotation)
        {
            var retlist = new List<Vector2> { Vector2.Zero };
            return retlist;
        }
        
    }

    /// <summary>
    /// Used for weapons which can be charged by holding a key
    /// </summary>
    public interface IChargable
    {
        float CurrentCharge { get; }

        byte CurrentPctCharge { get; }

        /// <summary>
        /// ElapsedTimeMS is the amount of time to charge the weapon at its current charge rate
        /// </summary>
        /// <param name="elapsedTimeMs"></param>
        void Charge(double elapsedTimeMs);

        /// <summary>
        /// Sets _currentCharge to 0
        /// </summary>
        void ResetCharge();

        Texture2D ChargeTex { get; }

    }
        

    /// <summary>
    /// Intented for battlecruiser
    /// Creates three Laser projectiles in front of the ship
    /// </summary>
    public class BC_Laser : Laser
    {
        public BC_Laser(ProjectileManager projectileManager, ICanFire holdingObj, byte slot)
            : base(projectileManager, holdingObj, slot)
        {
            Stats = new BC_LaserStats();
        }
                

        protected override List<Vector2> GeneratePositions(float rotation)
        {
            List<Vector2> positions = new List<Vector2>(Stats.NumProjectiles);
            positions.Add(OffsetBullet(HoldingObj.Position, HoldingObj.Rotation, 0, .4f));
            positions.Add(OffsetBullet(HoldingObj.Position, HoldingObj.Rotation, .2f, .2f));
            positions.Add(OffsetBullet(HoldingObj.Position, HoldingObj.Rotation, -.2f, .2f));

            return positions;
        }


    }


    

    
    

}
