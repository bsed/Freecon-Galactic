using System;
using System.Collections.Generic;
using Server.Managers;
using Freecon.Models.TypeEnums;
using SRServer.Services;
using Server.Models;
using Server.Models.Structures;
using Server.Models.Space;
using Core.Models;
using Server.Database;

namespace Server.Factories
{

    public class StructureFactory
    {
        static LocalIDManager _localIDManager;
        static GalaxyRegistrationManager _galaxyRegistrationManager;

        public static void Initialize(LocalIDManager idm, GalaxyRegistrationManager rm)
        {
            _localIDManager = idm;
            _galaxyRegistrationManager = rm;
        }


        /// <summary>
        /// Creates a structure
        /// </summary>
        /// <param name="type"></param>
        /// <param name="xPos"></param>
        /// <param name="yPos"></param>
        /// <param name="owner"></param>
        /// <param name="commandCenter"></param>
        /// <param name="currentAreaID"></param>
        /// <param name="pl"></param>
        /// <param name="writeToDB">If true, must specify dbm</param>
        /// <param name="dbm">must be specified if writeToDB is true</param>
        /// <returns></returns>
        public static IStructure CreateStructure(StructureTypes type, float xPos, float yPos, Player owner, CommandCenter commandCenter, int currentAreaID, IPlayerLocator pl, bool writeToDB=false, IDatabaseManager dbm = null)
        {
            IStructure s;
            switch (type)
            {
                case (StructureTypes.LaserTurret):
                    TurretTypes t = owner.GetArea().AreaType == AreaTypes.Planet ? TurretTypes.Planet : TurretTypes.Space;
                    s = new Turret(_localIDManager.PopFreeID(), xPos, yPos, owner.Id, currentAreaID, t, pl);
                   
                    break;
                case (StructureTypes.Biodome):
                    return CreateBiodome(xPos, yPos, owner, currentAreaID);
                case (StructureTypes.PowerPlant):
                    s = new PowerPlant(xPos, yPos, _localIDManager.PopFreeID(), owner.Id, currentAreaID);
                    break;
                case (StructureTypes.Silo):
                    s = new Silo(xPos, yPos, _localIDManager.PopFreeID(), owner.Id, currentAreaID);
                    break;
                case (StructureTypes.CommandCenter):
                    return CreateCommandCenter(xPos, yPos, owner, currentAreaID);
                case(StructureTypes.Factory):
                    s = new Factory(xPos, yPos, _localIDManager.PopFreeID(), owner.Id, currentAreaID);
                    break;
                    
                default:
                    throw new Exception("CreateStructure not implemented for structure type " + type.ToString());

            }
            _galaxyRegistrationManager.RegisterObject(s);            

            if(writeToDB)
            {
                if(dbm == null)
                {
                    throw new Exception("Error: must specify IDatabaseManager dbm if writeToDB is true.");
                }
                else
                {
                    dbm.SaveAsync(s);
                }
            }

            return s;

        }

        public static Biodome CreateBiodome(float xPos, float yPos, Player owner, int currentAreaID)
        {
            Biodome b = new Biodome(xPos, yPos, _localIDManager.PopFreeID(), owner.Id, currentAreaID);
            _galaxyRegistrationManager.RegisterObject(b);
            return b;
        }

        public static CommandCenter CreateCommandCenter(float xPos, float yPos, Player owner, int currentAreaID)
        {
            CommandCenter c = new CommandCenter(xPos, yPos, _localIDManager.PopFreeID(), owner, currentAreaID);
            _galaxyRegistrationManager.RegisterObject(c);
            return c;


        }

        /// <summary>
        /// For structures which originate as StatefulCargo, as they already have a GalaxyID
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static IStructure CreateStructure(StatefulCargo c, float xPos, float yPos, Player owner, CommandCenter commandCenter, int currentAreaID, IPlayerLocator pl)
        {
            IStructure s;
            switch (c.CargoType)
            {
                case (StatefulCargoTypes.LaserTurret):
                    {
                        TurretTypes t = owner.GetArea().AreaType == AreaTypes.Planet ? TurretTypes.Planet : TurretTypes.Space;
                        s = new Turret(c.Id, xPos, yPos, owner.Id, currentAreaID, t, pl);
                        break;
                    }
                case(StatefulCargoTypes.DefensiveMine):
                    {
                        s = new DefensiveMine(xPos, yPos, c.Id, owner.Id, currentAreaID, pl);
                        break;
                    }

                default:
                    throw new Exception("CreateStructure not implemented for structure type " + c.CargoType.ToString());

            }
            _galaxyRegistrationManager.RegisterObject(s);

            return s;

        }
              

        public static MineStructure CreateMine(float xPos, float yPos, Player owner, int currentAreaID, ICollection<ResourcePool> resourcePools)
        {
            MineStructure s = new MineStructure(xPos, yPos, _localIDManager.PopFreeID(), owner.Id, currentAreaID, resourcePools);
            _galaxyRegistrationManager.RegisterObject(s);
            return s;
        }



    }

}
