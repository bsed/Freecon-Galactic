using Freecon.Models.TypeEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Managers.Factories
{
    public struct ShipCreationProperties
    {
        public float PosX, PosY;

        public int AreaId;

        public ShipTypes ShipType;

        public PilotTypes PilotType;

        //Set to null or leave empty for no weapons. Order matters.
        public IEnumerable<WeaponTypes> WeaponTypes;

        public bool SetAsActiveShip;

        public ShipCreationProperties(float posX, float posY, int areaId, PilotTypes pilotType, ShipTypes shipType, IEnumerable<WeaponTypes> weaponTypes)
        {
            PosX = posX;
            PosY = posY;
            AreaId = areaId;
            PilotType = pilotType;
            ShipType = shipType;
            WeaponTypes = weaponTypes;
            SetAsActiveShip = false;
        }
    }
}
