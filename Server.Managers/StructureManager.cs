using Core.Models;
using Core.Models.CargoHandlers;
using Core.Models.Enums;
using Freecon.Models.TypeEnums;
using Server.Database;
using Server.Managers.Synchronizers.Transactions;
using Server.Models.Structures;
using SRServer.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Freecon.Core.Interfaces;
using Freecon.Models;

namespace Server.Managers
{
    public class StructureManager : ObjectManager<IStructure, IStructureModel>
    {
        ILocalIDManager _galaxyIDManager;
        CargoSynchronizer _cargoSynchronizer;
        private IAreaLocator _areaLocator;
        
        HashSet<int> _structuresToRemove = new HashSet<int>();

        public StructureManager(IDatabaseManager dbm, IAreaLocator areaLocator, ILocalIDManager galaxyIDManager, CargoSynchronizer cargoSynchronizer):base(dbm)
        {
            if (galaxyIDManager.IDType != IDTypes.GalaxyID)
                throw new Exception("Error: LocalIDManager required for StructureManager must be of IDTypes.GalaxyID");

            _galaxyIDManager = galaxyIDManager;
            _cargoSynchronizer = cargoSynchronizer;
            _areaLocator = areaLocator;
        }

        protected override IStructure _instantiateObject(IDBObject s, LocatorService ls)
        {
            return StructureHelper.InstantiateStructure((IStructureModel)s, ls.PlayerLocator, ls.RegistrationManager);
        }

        public async override Task Update(float currentTime)
        {
            float elapsedTime = currentTime - _lastUpdateTimeStamp;
            _structuresToRemove = new HashSet<int>();
            foreach(var s in _objects)
            {
                s.Value.Update(elapsedTime);

                if(s.Value.StructureType == StructureTypes.Factory)
                {
                    await _handleFactoryConstruction((Factory)s.Value);
                }
                else if (s.Value.StructureType == StructureTypes.LaserTurret && ((Turret)s.Value).IsDead)
                {
                    _areaLocator.GetArea(s.Value.CurrentAreaId).RemoveStructure(s.Value.Id, false);//Kill message sent by KillManager
                    _structuresToRemove.Add(s.Value.Id);
                }

            }

            foreach (var i in _structuresToRemove)
            {
                DeregisterObject(i);
            }
        }

        /// <summary>
        /// Converts from Constructables, if ready, to a cargo object, adding the cargo object to the factory's cargo.
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        async Task _handleFactoryConstruction(Factory f)
        {
            CargoTransaction tr = null;
            switch (f.CompletedPendingInstantiation)
            {
                case Constructables.Null:
                    tr = null;
                    break;
                    
                case Constructables.LaserTurret:
                    {
                        CargoLaserTurret t = new CargoLaserTurret(_galaxyIDManager.PopFreeID(), 666, new LaserWeaponStats());
                        tr = new TransactionAddStatefulCargo(f, t, false);
                        _cargoSynchronizer.RequestTransaction(tr);
                        await tr.ResultTask;
                        break;
                    }
                    

            }


            if (tr != null && tr.ResultTask.Result == CargoResult.Success)
            {
                f.CompletedPendingInstantiation = Constructables.Null;
            }

        }


    }
}
