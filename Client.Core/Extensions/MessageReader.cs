using Core.Models;
using FarseerPhysics.Dynamics;
using Freecon.Models;
using Freecon.Models.TypeEnums;
using Microsoft.Xna.Framework;
using Freecon.Client.Managers;
using Freecon.Client.Objects;
using System;
using Freecon.Core.Networking.Models.Objects;

namespace Freecon.Client.Core.Extensions
{
    public static class MessageReader
    {

        /// <summary>
        /// Set isPlayerShip if this is the client's ship, false otherwise.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="world"></param>
        /// <param name="clientShipManager"></param>
        /// <param name="isPlayerShip"></param>
        /// <returns></returns>
        public static Ship InstantiateShip(ShipData data, World world, ClientShipManager clientShipManager, bool isPlayerShip)
        { 
            ShipStats stats = new ShipStats();
            if(data.ShipStats != null)
            {
                stats = new ShipStats(data.ShipStats);   
            }

            Vector2 position = new Vector2(data.PosX, data.PosY);
            Vector2 velocity = new Vector2(data.VelX, data.VelY);
            

            Ship newShip = null;
            if (!isPlayerShip)
            {
                newShip = clientShipManager.CreateShip(world, data.IsNPC, position, data.Id, data.Rotation, velocity, data.PlayerName, stats, data.WeaponTypes, data.TeamIDs);

            }
            else
            {
                if(data.PlayerID == null)
                {
                    throw new NotImplementedException("Null playerID not yet implemented on the client.");
                }
                
                newShip = clientShipManager.CreatePlayerShip(world, position, data.Id, data.Rotation, velocity, data.PlayerName, stats, data.WeaponTypes, data.TeamIDs);


            }
            

            newShip.CurrentHealth = (int)data.CurrentHealth;
            newShip.Shields.CurrentShields = (int)data.CurrentShields;

            #region Cargo
            if (data.Cargo != null)
            {
                foreach (var sc in data.Cargo.StatelessCargo)
                {
                    newShip.Cargo.AddStatelessCargo(sc.CargoType, sc.Quantity, true);

                }

                foreach (var sc in data.Cargo.StatefulCargo)
                {

                    StatefulCargo c = InstantiateStatefulCargo(sc);
                    switch (c.CargoType)
                    {
                        case StatefulCargoTypes.Module:
                            {
                                newShip.AddModule((Module)c);
                                newShip.Cargo.AddStatefulCargo(c, true);
                                break;
                            }
                        default:
                            {
                                newShip.Cargo.AddStatefulCargo(c, true);
                                break;
                            }
                    }

                }


                newShip.RecalculateModuleBonuses();
            }
            #endregion

            newShip.MissileLauncher?.SetMissileType(data.SelectedMissileType);

            newShip.Pilot.IsAlive = !data.IsDead;

            return newShip;
        }
        
        public static Module InstantiateModule(CargoModuleData data)
        {
            Module retModule = null;

            int id = data.Id;
            byte level = data.Level;

            switch (data.ModuleType)
            {
                case ModuleTypes.EnergyRegenModule:
                    {
                        retModule = new EnergyRegenModule(id, level);
                        break;
                    }
                case ModuleTypes.MaxEnergyModule:
                    {
                        retModule = new MaxEnergyModule(id, level);
                        break;
                    }
                case ModuleTypes.MaxShieldModule:
                    {
                        retModule = new MaxShieldModule(id, level);
                        break;
                    }
                case ModuleTypes.ShieldRegenModule:
                    {
                        retModule = new ShieldRegenModule(id, level);
                        break;
                    }
                case ModuleTypes.ThrustModule:
                    {
                        retModule = new ThrustModule(id, level);
                        break;
                    }
                case ModuleTypes.DamageModule:
                    {
                        retModule = new DamageModule(id, level);
                        break;
                    }
                case ModuleTypes.DefenseModule:
                    {
                        retModule = new DefenseModule(id, level);
                        break;
                    }
                case ModuleTypes.TurnRateModule:
                    {
                        retModule = new TurnRateModule(id, level);
                        break;
                    }
                case ModuleTypes.LateralThrustModule:
                    {
                        retModule = new LateralThrustModule(id, level);
                        break;
                    }
                case ModuleTypes.TopSpeedModule:
                    {
                        retModule = new TopSpeedModule(id, level);
                        break;
                    }
                default:
                    throw new NotImplementedException("Module type not implemented in MessageReader.ReadModule");


            }

            return retModule;

        }


        public static StatefulCargo InstantiateStatefulCargo(StatefulCargoData data)
        {
            StatefulCargo c = null;
            switch (data.CargoType)
            {
                case StatefulCargoTypes.LaserTurret:
                    {
                        c = new CargoLaserTurret(data.Id, ((CargoLaserTurretData)data).Health, new LaserWeaponStats());
                        break;
                    }
                case StatefulCargoTypes.DefensiveMine:
                    {
                        c = new StatefulCargo(data.Id, StatefulCargoTypes.DefensiveMine);
                        break;
                    }
                case StatefulCargoTypes.Module:
                    {
                        c = InstantiateModule((CargoModuleData)data);
                        break;
                    }
                default:
                    Console.WriteLine("StatefulCargoType " + data.CargoType.ToString() + " not implemented in ReadNewShip.");
                    break;
            }

            return c;
        }



    }

}

