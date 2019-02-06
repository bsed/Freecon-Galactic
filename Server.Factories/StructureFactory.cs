using System;
using System.Collections.Generic;
using Server.Managers;
using System.Reflection;
using Freecon.Models.TypeEnums;
using SRServer.Services;
using Server.Models;
using Server.Models.Structures;
using Server.Models.Space;
using Core.Models;

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
        /// <param name="health"></param>
        /// <param name="xPos"></param>
        /// <param name="yPos"></param>
        /// <param name="owner"></param>
        /// <param name="commandCenter"></param>
        /// <returns></returns>
        public static IStructure CreateStructure(StructureTypes type, float xPos, float yPos, Player owner, CommandCenter commandCenter, int currentAreaID, IPlayerLocator pl)
        {
            IStructure s;
            switch (type)
            {
                case (StructureTypes.LaserTurret):
                    s = new Turret(_localIDManager.PopFreeID(), xPos, yPos, owner, commandCenter, currentAreaID, pl);                    
                    break;
                case (StructureTypes.Biodome):
                    s = new Biodome(xPos, yPos, _localIDManager.PopFreeID(), owner, currentAreaID);
                    break;
                case (StructureTypes.PowerPlant):
                    s = new PowerPlant(xPos, yPos, _localIDManager.PopFreeID(), owner, currentAreaID);
                    break;
                case (StructureTypes.Silo):
                    s = new Silo(xPos, yPos, _localIDManager.PopFreeID(), owner, currentAreaID);
                    break;               


                default:
                    throw new Exception("CreateStructure not implemented for structure type " + type.ToString());
                    
            }
            _galaxyRegistrationManager.RegisterObject(s);
            
            return s;

        }

        /// <summary>
        /// For structures which originate as StatefulCargo, as they already have a GalaxyID
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static IStructure CreateStructure(StatefulCargo c, float xPos, float yPos, Player owner, CommandCenter commandCenter, int currentAreaID, IPlayerLocator pl)
        {
            IStructure s;
            switch (c.Type)
            {
                case (StatefulCargoTypes.LaserTurret):
                    s = new Turret(_localIDManager.PopFreeID(), xPos, yPos, owner, commandCenter, currentAreaID, pl);
                    break;

                default:
                    throw new Exception("CreateStructure not implemented for structure type " + c.Type.ToString());
                    
            }
            _galaxyRegistrationManager.RegisterObject(s);

            return s;

        }


        public static Mine CreateMine(float xPos, float yPos, Player owner, int currentAreaID, ICollection<ResourcePool> resourcePools)
        {
            Mine s = new Mine(xPos, yPos, _localIDManager.PopFreeID(), owner, currentAreaID, resourcePools);
            _galaxyRegistrationManager.RegisterObject(s);
            return s;
        }

        

    }
    
}
