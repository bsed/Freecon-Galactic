using System;
using Core.Models.Enums;
using Freecon.Core.Interfaces;

namespace Server.Models.Space
{
    /// <summary>
    /// Represents a resource deposit on a planet (or maybe eventually in space) for greedy capitalist pig players to exploit
    /// </summary>
    public class ResourcePool:IHasPosition
    {
        public Resource Resource { get; protected set; }

        protected ResourceTypes ResourceType { get; set; }

        public float PosX { get; set; }
        public float PosY { get; set; }

        public float DepletedRate { get; set; }

        public ResourcePool()
        { }


        public ResourcePool(ResourceTypes resourceType, int numUnits, float depletedRate)
        {
            


            switch (resourceType)
            {
                case ResourceTypes.Bauxite:
                    Resource = new Bauxite();
                    break;
                case ResourceTypes.Hydrocarbons:
                    Resource = new Hydrocarbons();
                    break;

                case ResourceTypes.Hydrogen:
                    Resource = new Hydrogen();
                    break;

                case ResourceTypes.IronOre:
                    Resource = new IronOre();
                    break;

                case ResourceTypes.Medicine:
                    Resource = new Medicine();
                    break;

                case ResourceTypes.Organics:
                    Resource = new Organics();
                    break;

                case ResourceTypes.Silica:
                    Resource = new Silica();
                    break;

                case ResourceTypes.ThoriumOre:
                    Resource = new ThoriumOre();
                    break;

                default:
                    throw new Exception("Error: " + resourceType.ToString() + " not defined in ResourcePool constructor.");
            }


            Resource.AddResource(numUnits);
            DepletedRate = depletedRate;

        }

        


    }
}
