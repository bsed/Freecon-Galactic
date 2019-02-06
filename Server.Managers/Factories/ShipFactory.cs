using Freecon.Models.TypeEnums;
using Server.Database;
using Server.Interfaces;
using Server.Models;
using Server.Models.Interfaces;
using SRServer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Managers.Factories
{
    public class ShipFactory
    {
        IGalaxyRegistrationManager _galaxyRegistrationManager;
        WarpManager _warpManager;
        ILocalIDManager _galaxyIdManager;
        LocatorService _locatorService;
        IDatabaseManager _databaseManager;

        public ShipFactory(IGalaxyRegistrationManager rm, WarpManager wm, ILocalIDManager galaxyIdManager, LocatorService ls, IDatabaseManager dbm)
        {
            _galaxyRegistrationManager = rm;
            _warpManager = wm;
            _galaxyIdManager = galaxyIdManager;
            _locatorService = ls;
            _databaseManager = dbm;
        }

        /// <summary>
        /// Remember to call ship.SetPlayer and player.SetShip
        /// </summary>
        /// <param name="props"></param>
        /// <returns></returns>
        public IShip CreateShip(ShipCreationProperties props)
        {
            IShip tempShip = null;
            switch (props.PilotType)
            {
                case PilotTypes.Player:
                    tempShip = new PlayerShip(ShipStatManager.TypeToStats[props.ShipType], _locatorService);
                    break;
                case PilotTypes.NPC:
                    tempShip = new NPCShip(ShipStatManager.TypeToStats[props.ShipType], _locatorService);
                    break;
                case PilotTypes.Simulator:
                    throw new NotImplementedException("ShipFactory.CreateShip not yet implemented for PilotTypes.Simulator");
            }

            
            tempShip.Id = _galaxyIdManager.PopFreeID();
            _galaxyRegistrationManager.RegisterObject(tempShip);

            foreach (var w in props.WeaponTypes)
                tempShip.SetWeapon(WeaponManager.GetNewWeapon(w));

            tempShip.PosX = props.PosX;
            tempShip.PosY = props.PosY;
            tempShip.CurrentEnergy = tempShip.ShipStats.Energy;

            _warpManager.MoveShipLocal(tempShip, props.AreaId);

            return tempShip;
        }

        public void DeleteShip(int shipId)
        {
            //TODO: implement so that we don't pollute the db with out of play ships
            throw new NotImplementedException();
        }
    }
}
