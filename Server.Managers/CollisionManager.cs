using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Core.Models.Enums;
using Freecon.Models.TypeEnums;
using Server.Models.Interfaces;
using Server.Models;
using Freecon.Core.Utils;
using Server.Interfaces;
using Freecon.Models;
using SRServer.Services;

namespace Server.Managers
{
    public class CollisionManager
    {

        private ConcurrentDictionary<int, CollisionObject> _collisionsToCheck = new ConcurrentDictionary<int, CollisionObject>();
        private ConcurrentStack<CollisionObject> _preAllocatedCollisionObjects = new ConcurrentStack<CollisionObject>();

        private ConcurrentDictionary<int, ICollidable> _collidableObjects;//Anything that can be hit by a projectile must be registered here

        IAreaLocator _galaxyManager;
        MessageManager _messageManager;
        KillManager _killManager;
        ProjectileManager _projectileManager;
        Random r = new Random(6546543);

        public CollisionManager(IAreaLocator galaxyManager, MessageManager messageManager, KillManager km, ProjectileManager pm)
        {

            for (int i = 0; i < 100000; i++) //Fill the projStack with IDs
            {
                _preAllocatedCollisionObjects.Push(new CollisionObject());

            }

            _collidableObjects = new ConcurrentDictionary<int, ICollidable>();
            _galaxyManager = galaxyManager;
            _messageManager = messageManager;
            _killManager = km;
            _projectileManager = pm;

        }

        public void Update(GalaxyRegistrationManager rm, ProjectileManager pm)
        {
            handleCollisions(rm, pm);
        }

        /// <summary>
        /// Registers the provided object. If the Id has already been registered, the value is updated with the passed obj
        /// </summary>
        /// <param name="obj"></param>
        public void RegisterCollidableObject(ICollidable obj)
        {
            _collidableObjects.AddOrUpdate(obj.Id, obj, (k, v) => obj);
            
        }

        /// <summary>
        /// Checks if the key is currently registered to CollidableObjects
        /// </summary>was shot 
        /// <param name="key"></param>
        /// <returns></returns>
        public bool CheckCollidableKey(int key)
        {
            return _collidableObjects.ContainsKey(key);

        }

        public bool TryRemoveCollidableKey(int key)
        {
            ICollidable c;
            return(_collidableObjects.TryRemove(key, out c));
                    
            
        }

        /// <summary>
        /// Use this for reported collision, to add for majority resolution
        /// If a collision has already been added, it will automatically be processed (since two clients reported the collision)
        /// </summary>
        /// <param name="projectileID"></param>
        public void CreateCollision(int projectileID, ProjectileTypes projectileType, int hitObjectID, byte pctCharge, byte weaponSlot)
        {                       

            //If _collidableObjects does not contain hitObjectID, something is probably wrong.
            //If ProjectileManager does not contain projectileID, the collision is probably from a silently denied projectile, which should be rare
            //WARNING: Could optimize by telling client not to report collisions with denied projectiles
            if (!_collidableObjects.ContainsKey(hitObjectID) || !_projectileManager.idToProjectile.ContainsKey(projectileID))//In case a collision is reported right after a kill
            {
                if (!_collidableObjects.ContainsKey(hitObjectID))
                    ConsoleManager.WriteLine("Error: collision reported for an object which was not registered with CollisionManager.", ConsoleMessageType.Error);

                return;
            }
            ICollidable hitObject = _collidableObjects[hitObjectID];

            if (hitObject.CurrentAreaId == null)//Received a collision report while an object was mid warp, ignore.
                return;

            CollisionObject tempCol;
            try
            {
                if (_collisionsToCheck.TryGetValue(projectileID, out tempCol)) //If the collision has been reported 
                {
                    tempCol.numReported++;

                    //This means more than one client reported the collision, so always take damage
                    if (!tempCol.isExpired) //If it has not been handled yet
                    {
                        ConsoleManager.WriteLine("Damaging, multiple reports, id  " + projectileID);
                        DamageObject(hitObject, _projectileManager.idToProjectile[projectileID].FiringObject, projectileType, pctCharge, weaponSlot, projectileID);

                        tempCol.isExpired = true;
                    }
                }
                else if (_galaxyManager.GetArea(hitObject.CurrentAreaId).NumOnlinePlayers == 1)
                //If there is only one player in the system (e.g. a player simulating npcs alone)
                {
                    ConsoleManager.WriteLine("Damaging, single user, id  " + projectileID);
                    tempCol = getColObject(projectileID, projectileType, hitObject, pctCharge, weaponSlot);
                    _collisionsToCheck.TryAdd(projectileID, tempCol);
                    DamageObject(hitObject, _projectileManager.idToProjectile[projectileID].FiringObject, projectileType, pctCharge, weaponSlot, projectileID);//Take damage
                    tempCol.isExpired = true;
                }

                else //If the collision has not been reported
                {
                    //Program.numCollided++;

                    tempCol = getColObject(projectileID, projectileType, hitObject, pctCharge, weaponSlot);
                    _collisionsToCheck.TryAdd(projectileID, tempCol);
                }
            }
            catch(Exception e)
            {
                //TODO: figure out a way to deal with hitObject.CurrentAreaID==null, which can happen if a collision is created while the object is warping. Currently not concurrency safe.
                ConsoleManager.WriteLine(e.Message, ConsoleMessageType.Error);
                ConsoleManager.WriteLine(e.StackTrace, ConsoleMessageType.Error);
            }
            hitObject.DoCombatUpdates = true;
            hitObject.TimeOfLastCollision = TimeKeeper.MsSinceInitialization;
        }

        protected void handleCollisions(GalaxyRegistrationManager rm, ProjectileManager pm)
        {
          
            ICollidable hitObj;
            var keysToRemove = new List<int>();

            foreach (var kvp in _collisionsToCheck)
            {
                hitObj = kvp.Value.hitObject;
                int numOnline = 0;

                try
                {
                    //This line for some reason causes null reference exceptions, although none of the variables are null
                    numOnline = _galaxyManager.GetArea(hitObj.CurrentAreaId).NumOnlinePlayers;
                }
                catch (Exception e)
                {
                    ConsoleManager.WriteLine("Mysterious null error in CollisionManager.HandleCollisions.", ConsoleMessageType.Warning);
                    continue;

                }

                if (kvp.Value.isExpired && (TimeKeeper.MsSinceInitialization - kvp.Value.reportTime) >= 3000)
                {    //If it has been handled and the grace period for further reports has ended
                    keysToRemove.Add(kvp.Value.projectileID);
                }
                else if (kvp.Value.isExpired == false &&
                        (TimeKeeper.MsSinceInitialization - kvp.Value.reportTime) >= 200)
                //Only one collision reported
                {
                    //ConsoleManager.WriteToFreeLine("Single Reported");
                    //Program.numSingleCols++;
                    float probability = (1f / numOnline) * 100; //probability that the collision is valid

                    if (r.Next(0, 100) <= probability)
                    {
                        if (_projectileManager.idToProjectile.ContainsKey(kvp.Value.projectileID))
                        {
                            DamageObject(hitObj, _projectileManager.idToProjectile[kvp.Value.projectileID].FiringObject, kvp.Value.projectileType, kvp.Value.pctCharge, kvp.Value.WeaponSlot, kvp.Value.projectileID);//Collision considered valid
                        }
                        else
                        {
                            ConsoleManager.WriteLine("Error: projectileID not found for projectile collision. Skipping damage...", ConsoleMessageType.Error);
                        }
                    }

                    kvp.Value.isExpired = true;
                }
            }


#if DEBUG
            //TODO: Remove this before release
            for (int i = 0; i < keysToRemove.Count - 1; i++)
            {
                for (int j = i + 1; j < keysToRemove.Count; j++)
                {
                    if (keysToRemove[i] == keysToRemove[j])
                    {
                        ConsoleManager.WriteLine("SAME KEY ADDED TWICE1!!!!");
                        throw new Exception("SAME KEY ADDED TWICE1!!!!");

                    }
                }

            }
#endif
            try
            {
                for (int i = 0; i < keysToRemove.Count; i++)
                {
                    CollisionObject tempCol;
                    //Program.numReports -= _collisionsToCheck[keysToRemove[i]].numReported;
                    recycleCollisionObject(_collisionsToCheck[keysToRemove[i]]);
                    _collisionsToCheck.TryRemove(keysToRemove[i], out tempCol);
                }
            }
            catch
            {
                Console.WriteLine("bsdfjlehliau");

            }
            
        }

        /// <summary>
        /// Does nothing if object is dead
        /// </summary>
        /// <param name="hitObject"></param>
        /// <param name="firingObject"></param>
        /// <param name="projectileType"></param>
        /// <param name="pctCharge"></param>
        /// <param name="weaponSlot"></param>
        /// <param name="projectileID"></param>
        void DamageObject(ICollidable hitObject, ICanFire firingObject, ProjectileTypes projectileType, byte pctCharge, byte weaponSlot, int projectileID)
        {                       

            hitObject.TimeOfLastCollision = TimeKeeper.MsSinceInitialization;
            float multiplier = 1;
            if(firingObject is IShip)
            {
                multiplier = ((IShip)firingObject).StatBonuses[StatBonusTypes.Damage] / (1+((IShip)firingObject).Debuffs[DebuffTypes.Damage]);
            }

            Weapon firingWeapon = firingObject.GetWeapon(weaponSlot);
            Dictionary<DebuffTypes, int> debuffsAdded = null;
            if(firingWeapon != null && firingWeapon.DebuffType != DebuffTypes.None)
            {
                debuffsAdded = new Dictionary<DebuffTypes, int>();
                debuffsAdded.Add(firingWeapon.DebuffType, firingWeapon.DebuffCount);
                hitObject.Debuffs.AddDebuff(firingWeapon.DebuffType, firingWeapon.DebuffCount, TimeKeeper.MsSinceInitialization);
            }

            //WARNING: Concurrent processing may cause this to take damage more than once for the same collision
            bool isDead = hitObject.TakeDamage(projectileType, pctCharge, multiplier);

            if (isDead && hitObject is IKillable)
            {
                _killManager.Kill((IKillable)hitObject, firingObject);
            }
            else
            {
                if (hitObject is IShip)
                {

                    _messageManager.SendShipDamage((IShip)hitObject, debuffsAdded);
                }
            }

        }





        /// <summary>
        /// Represents a collision, used to process collisions
        /// </summary>
        private class CollisionObject
        {
            public bool isExpired = false;
            //Used to keep track of collisions which have already been handled, in case any other reports come in late

            public int numReported = 1;
            public int projectileID;
            public ProjectileTypes projectileType;
            public float reportTime; //Time of report, in milliseconds
            public ICollidable hitObject;
            public byte pctCharge;
            public byte WeaponSlot;

            
        }

        private CollisionObject getColObject(int projectileID, ProjectileTypes projectileType, ICollidable hitObject, byte pctCharge, byte weaponSlot)
        {
            CollisionObject tempCol;
            if (!_preAllocatedCollisionObjects.TryPop(out tempCol))
            {
                for (int i = 0; i < 500; i++) //Every time the capacity is exceeded, create 500 more objects.
                {
                    _preAllocatedCollisionObjects.Push(new CollisionObject());
                }
                _preAllocatedCollisionObjects.TryPop(out tempCol);
            }
            tempCol.projectileID = projectileID;
            tempCol.projectileType = projectileType;
            tempCol.hitObject = hitObject;
            tempCol.reportTime = TimeKeeper.MsSinceInitialization;
            tempCol.pctCharge = pctCharge;
            tempCol.WeaponSlot = weaponSlot;
            return tempCol;
        }

        /// <summary>
        /// Pushes the unused collisionObject back onto the stack of free objects
        /// </summary>
        private void recycleCollisionObject(CollisionObject col)
        {
            _preAllocatedCollisionObjects.Push(col);
            col.numReported = 1;
            col.isExpired = false;
            col.reportTime = 0;
        }



    }
}
