using System;
using Freecon.Models.TypeEnums;
using Server.Models.Interfaces;
using SRServer.Services;

namespace Server.Models.Structures
{
    public abstract class StructureHelper
    {
        // Need to find a better place for this...
        public static IStructure InstantiateStructure(IStructureModel sm, IPlayerLocator pl, IGalaxyRegistrationManager gm)
        {
            IStructure s;
            switch (sm.StructureType)
            {
                case (StructureTypes.LaserTurret):
                    s = new Turret((TurretModel)sm, pl);
                    break;
                case (StructureTypes.Biodome):
                    s = new Biodome((BiodomeModel)sm);
                    break;
                case (StructureTypes.PowerPlant):
                    s = new PowerPlant((PowerPlantModel)sm);
                    break;
                case (StructureTypes.Silo):
                    s = new Silo((SiloModel)sm);
                    break;
                case (StructureTypes.CommandCenter):
                    return new CommandCenter((CommandCenterModel)sm);
                case (StructureTypes.Factory):
                    s = new Factory((FactoryModel)sm);
                    break;
                case StructureTypes.Refinery:
                    s = new Refinery((RefineryModel)sm);
                    break;
                case StructureTypes.Mine:
                    s = new MineStructure((MineModel)sm);
                    break;
                case StructureTypes.DefensiveMine:
                    s = new DefensiveMine((DefensiveMineModel)sm, pl);
                    break;
                case StructureTypes.ConstructionBuilding:
                    s = new ConstructionBuilding((ConstructionBuildingModel)sm);
                    break;
                default:
                    throw new Exception("CreateStructure not implemented for structure type " + sm.StructureType.ToString());

            }
            gm.RegisterObject(s);


            return s;


        }
    }
}