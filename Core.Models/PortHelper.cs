using Core.Models.Enums;
using Freecon.Core.Models.Enums;
using Freecon.Models.TypeEnums;
using System;

namespace Freecon.Models
{
    public class PortHelper
    {

        public static PortGoodCategory GetPortGoodCategory(PortWareIdentifier cargoType)
        {
            switch (cargoType)
            {
                case PortWareIdentifier.LaserTurret:
                case PortWareIdentifier.DefensiveMine:
                    return PortGoodCategory.Defenses;

                case PortWareIdentifier.Module:
                    return PortGoodCategory.Module;

                case PortWareIdentifier.PlasmaCannon:
                case PortWareIdentifier.Laser:
                case PortWareIdentifier.EnergyDisruptor:
                    return PortGoodCategory.Weapon;

                case PortWareIdentifier.Hull1:
                case PortWareIdentifier.Hull2:
                case PortWareIdentifier.Hull3:
                case PortWareIdentifier.Engine1:
                case PortWareIdentifier.Engine2:
                case PortWareIdentifier.Engine3:
                    return PortGoodCategory.Components;


                //Ships
                //Not sure if we'll actually allow ships to carry other ships: but this simplifies port buying/selling of ships
                case PortWareIdentifier.Penguin:
                case PortWareIdentifier.Reaper:
                case PortWareIdentifier.Battlecruiser:
                case PortWareIdentifier.Barge:
                    return PortGoodCategory.Ship;


                case PortWareIdentifier.Cash:
                case PortWareIdentifier.Hydrogen:
                case PortWareIdentifier.Iron:
                case PortWareIdentifier.Neutronium:
                case PortWareIdentifier.Hydrocarbons:
                case PortWareIdentifier.Woman:
                case PortWareIdentifier.ThoriumOre:
                case PortWareIdentifier.Thorium:
                case PortWareIdentifier.IronOre:
                case PortWareIdentifier.Organics:
                case PortWareIdentifier.Bauxite:
                case PortWareIdentifier.Silica:
                case PortWareIdentifier.Water:
                case PortWareIdentifier.Oxygen:
                case PortWareIdentifier.Opium:
                    return PortGoodCategory.Resource;

                case PortWareIdentifier.AmbassadorMissile:
                case PortWareIdentifier.HellHoundMissile:
                    return PortGoodCategory.Consumables;

                case PortWareIdentifier.Biodome:
                case PortWareIdentifier.CargoHold:
                    return PortGoodCategory.Components;
                default:
                    return PortGoodCategory.NotForSale;
            }
                                
        }

        /// <summary>
        /// TODO: consider hardcoding values, or preloading a dictionary, to avoid parsing strings?
        /// </summary>
        /// <param name="cargoType"></param>
        /// <returns></returns>
        public static PortWareIdentifier GetPortWareIdentifier(StatelessCargoTypes cargoType)
        {
            PortWareIdentifier identifier;           
            return Enum.TryParse(cargoType.ToString(), out identifier) ? identifier : PortWareIdentifier.Null;
        }

        /// <summary>
        /// TODO: consider hardcoding values, or preloading a dictionary, to avoid parsing strings?
        /// </summary>
        /// <param name="cargoType"></param>
        /// <returns></returns>
        public static PortWareIdentifier GetPortWareIdentifier(StatefulCargoTypes cargoType)
        {
            PortWareIdentifier identifier;
            return Enum.TryParse(cargoType.ToString(), out identifier) ? identifier : PortWareIdentifier.Null;
        }

        public static PortWareIdentifier GetPortWareIdentifier(PortServiceTypes serviceType)
        {
            PortWareIdentifier identifier;
            return Enum.TryParse(serviceType.ToString(), out identifier) ? identifier : PortWareIdentifier.Null;
        }

        public static bool GetCargoType(PortWareIdentifier wareType, out StatefulCargoTypes cargoType)
        {
            bool success = Enum.TryParse(wareType.ToString(), out cargoType);
            if(!success)
            {
                cargoType = StatefulCargoTypes.Null;
            }

            return success;
            
        }

        public static bool GetCargoType(PortWareIdentifier wareType, out StatelessCargoTypes cargoType)
        {
            bool success = Enum.TryParse(wareType.ToString(), out cargoType);
            if (!success)
            {
                cargoType = StatelessCargoTypes.Null;
            }

            return success;
        }

        public static bool GetServiceType(PortWareIdentifier wareType, out PortServiceTypes serviceType)
        {
            bool success = Enum.TryParse(wareType.ToString(), out serviceType);
            if (!success)
            {
                serviceType = PortServiceTypes.Null;
            }

            return success;
        }

    }
}
