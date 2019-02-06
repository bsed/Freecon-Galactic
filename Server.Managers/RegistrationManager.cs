using System;
using Server.Models.Interfaces;
using Server.Models;
using Server.Interfaces;
using Server.Models.Structures;
using Core.Models.Enums;
using Freecon.Core.Interfaces;

namespace Server.Managers
{

    /// <summary>
    /// Automatically handles registration of objects with GalaxyIDs
    /// TODO: Make concurrency safe?
    /// </summary>
    public class GalaxyRegistrationManager: IGalaxyRegistrationManager
    {
        GalaxyManager _galaxyManager;
        ShipManager _shipManager;
        CollisionManager _collisionManager;
        StructureManager _structureManager;

        LocalIDManager _galaxyIDManager;

        PlayerManager _playerManager;

        AccountManager _accountManager;
        CargoSynchronizer _cargoSynchronizer;

        public GalaxyRegistrationManager(GalaxyManager gm, ShipManager sm, CollisionManager cm, LocalIDManager idm, PlayerManager pm, AccountManager am, CargoSynchronizer cargoSynchronizer, StructureManager structureManager)
        {
            _galaxyManager = gm;
            _shipManager = sm;
            _collisionManager = cm;
            _galaxyIDManager = idm;
            _playerManager = pm;
            _accountManager = am;
            _cargoSynchronizer = cargoSynchronizer;
            _structureManager = structureManager;

            if(idm.IDType != IDTypes.GalaxyID)
                throw new Exception("Error: " + idm.GetType().ToString() + " must be of type " + IDTypes.GalaxyID + " in " + this.GetType().ToString());

        }

        /// <summary>
        /// Registers an object appropriately. ID must be added before calling this method.
        /// Use this for any non-temporary galaxy object (ships, planets, etc)
        /// If object has already been registered, it is updated with the new provided version
        /// </summary>
        /// <param name="obj"></param>
        public void RegisterObject(IHasGalaxyID obj)
        {
            //Need to add players here, maybe accounts

            if (obj is ICollidable)
                _collisionManager.RegisterCollidableObject((ICollidable)obj);

            if (obj is ISimulatable)
                _galaxyManager.RegisterSimulatable((ISimulatable)obj);

            _galaxyManager.AllObjects.AddOrUpdate(obj.Id, obj, (k, v) => obj);

            if (obj is IArea)
            {
                _galaxyManager.RegisterArea((IArea)obj);
                
            
            
            }
            if (obj is IShip)
                _shipManager.RegisterShip((IShip)obj);

            if (obj is IStructure)
                _structureManager.RegisterObject((IStructure)obj);

        }  

        public void RegisterObject(Player p)
        {
            _playerManager.RegisterObject(p);
        }

        public void RegisterObject(Account a)
        {
            _accountManager.RegisterAccount(a);
        }

        public void DeRegisterObject(IHasGalaxyID obj)
        {

            ISimulatable tempSim;
            IHasGalaxyID tempGal;

            //Maybe a type check is faster than TryRemove?

            _collisionManager.TryRemoveCollidableKey(obj.Id);

            _galaxyManager.DeRegisterArea(obj.Id);
            _galaxyManager.AllSimulatableObjects.TryRemove(obj.Id, out tempSim);
            _galaxyManager.AllObjects.TryRemove(obj.Id, out tempGal);


            _shipManager.DeregisterShip(obj.Id);

            _galaxyIDManager.PushFreeID(obj.Id);
        }


        public void DeRegisterObject(Player p)
        {
            _playerManager.DeregisterObject(p.Id);
        }
    }
}
