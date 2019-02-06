using Freecon.Models.TypeEnums;
using System;

namespace Freecon.Models
{
    public class ModuleFactory
    {
        public static Module CreateModule(ModuleTypes moduleType, int id, byte level)
        {
            Module retModule = null;
            switch (moduleType)
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
    }
}
