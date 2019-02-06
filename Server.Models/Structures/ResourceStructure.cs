using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System.Collections.Generic;
using Core.Models.Enums;

namespace Server.Models.Structures
{
    public class ResourceStructure<T, S> : Structure<T, S>
        where T:ResourceStructureModel<S>, new()
        where S:StructureStats, new()
    {
        public HashSet<ResourceTypes> ConsumedResources { get; protected set; }

        public override bool IsResourceStructure { get { return true; } }

        public ResourceStructure(float xPos, float yPos, int galaxyID, int ownerID, int currentAreaID)
            : base(xPos, yPos, galaxyID, ownerID, currentAreaID)
        {
            ConsumedResources = new HashSet<ResourceTypes>();
            
        }

        public ResourceStructure(T model)
        {
            ConsumedResources = new HashSet<ResourceTypes>();
            _model = model;
        }

        protected ResourceStructure()
        {
            ConsumedResources = new HashSet<ResourceTypes>();
        }

        /// <summary>
        /// Returns false if structure is full, with the unstored difference returned in numToStore
        /// </summary>
        /// <param name="type"></param>
        /// <param name="numToStore"></param>
        /// <returns></returns>
        public virtual bool StoreResources(ResourceTypes type, ref float numToStore)
        {

            if (_model.IsFull)
            {
                return false;
            }
            else if (_model.UnitsStored + numToStore <= _model.Capacity)
            {
                _model.Resources[type].AddResource(numToStore);//Room for all numToStore available
                _model.UnitsStored += numToStore;
                numToStore = 0;

            }
            else if (_model.UnitsStored == _model.Capacity)
            {
                _model.IsFull = true;
                return false;//silo full
            }
            else
            {
                _model.Resources[type].AddResource(_model.Capacity - _model.UnitsStored);//Store only as much as possible
                _model.UnitsStored = _model.Capacity;
                numToStore = numToStore - (_model.Capacity - _model.UnitsStored);
            }

            return _model.UnitsStored != _model.Capacity;


        }

        /// <summary>
        /// Returns number of resources available
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual float GetResourceAmount(ResourceTypes type)
        {
            return _model.Resources[type].NumUnits;
        }

        /// <summary>
        /// Returns true if amount of given resourcetype is stored, false otherwise
        /// </summary>
        /// <param name="type"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public virtual bool CheckResource(ResourceTypes type, float amount)
        {
            return _model.Resources[type].NumUnits >= amount;

        }

        /// <summary>
        /// Returns true if removal was succesful, false if there was not enough to remove
        /// </summary>
        /// <param name="type"></param>
        /// <param name="Amount"></param>
        /// <returns></returns>
        public virtual bool RemoveResources(ResourceTypes type, float amount)
        {
            if (amount < 0)
                return false;

            if (_model.Resources[type].NumUnits >= amount)
            {
                _model.Resources[type].TakeResource(amount);
                return true;
            }
            else
                return false;


        }
       
    }

    public class ResourceStructureModel<T>:StructureModel<T>
        where T:StructureStats, new()
    {
        public bool IsFull { get; set; }
        public float Capacity { get; set; }
        public float UnitsStored { get; set; }

        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<ResourceTypes, Resource> Resources { get; set; }
    
        protected ResourceStructureModel()
        {
            Resources = new Dictionary<ResourceTypes, Resource>();
        }

        public ResourceStructureModel(ResourceStructureModel<T> model)
        {
            IsFull = model.IsFull;
            Capacity = model.Capacity;
            UnitsStored = model.UnitsStored;
            Resources = model.Resources;
        }
    
    
    }

}
