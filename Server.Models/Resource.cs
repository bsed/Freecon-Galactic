using System.Collections.Generic;
using Core.Models.Enums;
using Freecon.Core.Interfaces;

namespace Server.Models
{
    public abstract class Resource
    {
        public ResourceTypes Type;
        public float WeightPerUnit;//Weight of each unit
        public int CargoPerUnit;//Amount of cargo each takes up
        public float NumUnits { get; protected set; }//Number of units of resource
        public float CurrentRate = 0;//Units per ms, rate of change
        public float DepletedRate { get; set; }//Used by mines, probably could be moved elsewhere...


        /// <summary>
        /// Attempts to take amount resource, returns the resulting amount. amount cannot be negative.
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public float TakeResource(float amount)
        {
            if (NumUnits > amount)
            {
                NumUnits -= amount;
                return amount;
            }
            else if (NumUnits > 0)
            {
                float retAmount = amount - NumUnits;
                NumUnits = 0;
                return retAmount;
            }
            else
            {
                return 0;
            }

        }

        public void AddResource(float amount)
        {
            NumUnits += amount;

        }

        public Resource GetClone()
        {
            return (Resource)this.MemberwiseClone();
        }
                
        
    }

    #region Resources

    public abstract class RawResource : Resource
    {

    }

    public abstract class RefinedResource : Resource
    {


    }

    public class IronOre : RawResource
    {
        public IronOre()
        {
            Type = ResourceTypes.IronOre;
        }

    }

    public class ThoriumOre : RawResource
    {
        public ThoriumOre()
        {
            Type = ResourceTypes.ThoriumOre;
        }

    }

    public class Hydrogen : RawResource
    {
        public Hydrogen()
        {
            Type = ResourceTypes.Hydrogen;
        }

    }

    public class Organics : RawResource
    {
        public Organics()
        {
            Type = ResourceTypes.Organics;
        }

    }

    public class Bauxite : RawResource
    {

        public Bauxite()
        {
            Type = ResourceTypes.Bauxite;
        }
    }

    public class Silica : RawResource
    {
        public Silica()
        {
            Type = ResourceTypes.Silica;
        }

    }

    


    public class ThoriumRefined : RefinedResource
    {
       

    }

    public class Steel : RefinedResource
    {
       

    }

    public class Hydrocarbons : RefinedResource
    {
        public Hydrocarbons()
        {
            Type = ResourceTypes.Hydrocarbons;
        }

    }

    public class Medicine : RefinedResource
    {
        public Medicine()
        {
            Type = ResourceTypes.Medicine;
        }

    }

    public class Aluminum : RefinedResource
    {


    }

    public class Silicon : RefinedResource
    {


    }

    public class Fullerene : RefinedResource
    {


    }

    //Fuel for ships?
    //Out of fuel -> run at very low power
    public class AntiMatter : RefinedResource
    {


    }

    #endregion

    ///// <summary>
    ///// Structures which can accept and provide resources
    ///// </summary>
    //public interface IResourceStore
    //{
    //    //float SupplyRateAvailable { get; set; }
    //    //float ThoriumSupplyAvailable { get; set; }

    //    int NumDrones { get; }
    //    float SupplyRatePerDrone { get; }

    //    /// <summary>
    //    /// Returns true if at least amount of given resourcetype is stored, false otherwise
    //    /// </summary>
    //    /// <param name="type"></param>
    //    /// <param name="amount"></param>
    //    /// <returns></returns>
    //    bool CheckResource(ResourceTypes type, float amount);

        

    //    /// <summary>
    //    /// Sets all resource rates to 0
    //    /// </summary>
    //    //void ResetResourceRates();

    //    //void ChangeResourceRate(ResourceTypes type, float changeAmount);

    //}

    /// <summary>
    /// Structures which can only accept resources
    /// </summary>
    public interface IResourceSink: IHasPosition
    {
        bool Enabled { get; }

        //ResourceHandler Resources { get; }

        /// <summary>
        /// Returns false if structure is full, with the unstored difference returned in numToStore
        /// </summary>
        /// <param name="type"></param>
        /// <param name="numToStore"></param>
        /// <returns></returns>
        bool StoreResources(ResourceTypes type, ref float amount);

        //The amount of each resource consumed per update
        HashSet<ResourceTypes> ConsumedResources { get; }
    }

    public interface IResourceSupply : IHasPosition
    {
        bool Enabled { get; }

        void Disable(string message);

        void Enable();

        float GetSupplyRate(float distance);

        /// <summary>
        /// Use multiplier to pass slider value as a percentage
        /// </summary>
        /// <param name="elapsedMS"></param>
        /// <param name="resourceSinks"></param>
        /// <param name="multiplier"></param>
        void SupplyUpdate(float elapsedMS, ICollection<IResourceSink> resourceSinks, float multiplier);

       //Dictionary<ResourceTypes, RawResource> Resources { get; set; }

    }
}
