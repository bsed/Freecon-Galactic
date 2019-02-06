using System.Collections.Generic;
using System;
using Freecon.Models.TypeEnums;
using Server.Models.Extensions;
using System.ComponentModel.DataAnnotations;
using Core.Models.Enums;
using Freecon.Core.Interfaces;
using Freecon.Core.Networking.Models.Objects;



namespace Server.Models.Structures
{
    public abstract class Structure<T, S> : IStructure
        where S : StructureStats, new()
        where T : StructureModel<S>, new()
    {
        protected T _model;

        public int? OwnerID { get { return _model.OwnerID; } set { _model.OwnerID = value; } }
        
        public int? CurrentAreaId { get { return _model.CurrentAreaID; } }

        public bool IsDead { get { return _model.IsDead; } set { _model.IsDead = value; } }

        public bool Enabled { get { return _model.Enabled; } }//Allows colony owner to disable buildings so that they don't take energy        

        public int Id { get { return _model.Id; } set { _model.Id = value; } }

        public virtual bool IsResourceStructure { get { return false; } }

        public float CurrentHealth { get { return _model.CurrentHealth; } }
        /// <summary>
        /// Set by colony
        /// </summary>
        public float DamageMultiplier { get { return _model.DamageMultiplier; } set { _model.DamageMultiplier = value; } }

        public float PosX { get { return _model.XPos; } set { _model.XPos = value; } }

        public float PosY { get { return _model.YPos; } set { _model.YPos = value; } }

        public int PeopleRequired { get { return _model.Stats.PeopleRequired; } set { _model.Stats.PeopleRequired = value; } }

        public float PowerConsumptionRate { get { return _model.Stats.PowerConsumptionRate; } set { _model.Stats.PowerConsumptionRate = value; } }

        public Weapon Weapon { get { return _model.Weapon; } set { _model.Weapon = value; } }

        public S Stats { get { return _model.Stats; } set { _model.Stats = value; } }

        public StructureTypes StructureType { get { return Stats.StructureType; } }

        public HashSet<ProblemFlagTypes> ProblemFlags { get { return _model.ProblemFlags; } protected set { _model.ProblemFlags = value; } }

        protected Structure()
        {
            _model = new T();
        }

        protected Structure(float xPos, float yPos, int galaxyID, int ownerID, int currentAreaID)
        {
            _model = new T();
            _model.XPos = xPos;
            _model.YPos = yPos;
            _model.CurrentHealth = 666;
            _model.OwnerID = ownerID;
            _model.CurrentAreaID = currentAreaID;
            _model.Enabled = true;
            Id = galaxyID;

        }

        protected Structure(T cm)
        {
            _model = cm;
        }
               

        public virtual float Update(float elapsedMS) //Each updatable building must have an update function to handle updating
        {
            //if (powerAvailable >= MaxPowerAvailable)
            //powerAvailable = MaxPowerAvailable;//To avoid "stockpiling" of excess energy

            return 0;

        }       
        
        public virtual void Enable()
        {
            _model.Enabled = true;

        }

        public virtual void Disable(string message)
        {
            _model.Enabled = false;
            _model.DisableMessage = message;

        }      

        public virtual StructureData GetNetworkData()
        {
            return new StructureData
            {
                CurrentHealth = CurrentHealth,
                Id = Id,
                StructureType = StructureType,
                XPos = PosX,
                YPos = PosY
            };
        }

        public virtual void Kill(int projectileId)
        {


        }   

        /// <summary>
        /// Returns the maximum available supply rate as a function of distance
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        public virtual float GetSupplyRate(float distance)
        {
            return 0;

        }

        #region Reference Getters and Setters               

        public IStructure SetArea(IArea newArea)
        {
            if (newArea != null)
            {
                _model.CurrentAreaID = newArea.Id;
            }
            else
            {
                _model.CurrentAreaID = null;
            }

            return this;
        }

        public IStructure SetPlayer(Player p)
        {
            if (p != null)
            {
                _model.OwnerID = p.Id;
            }
            else
            {
                _model.OwnerID = null;
            }

            return this;
        }

        public void SetID(int ID)
        {
            Id = ID;
        }

        /// <summary>
        /// Handles setting of simulating player
        /// Command to begin simulating must be sent after this function is called
        /// </summary>
        /// <param name="p"></param>
        public virtual void SetSimulatingPlayer(Player p)
        {
            if (p != null)
            {
                _model.SimulatingPlayerID = p.Id;
                return;
            }

            _model.SimulatingPlayerID = null;
        }

        #endregion

        /// <summary>
        /// Returns true if objects overlap, false otherwise.
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns></returns>
        public bool CheckOverlap(float sizex, float sizey, float posx, float posy)
        {
            // Keeping it simple for now, all structures are rectangles
            return Math.Abs(PosX - posx) < ((Stats.StructureSizeX + sizex) / 2) &&
                   Math.Abs(PosY - posy) < ((Stats.StructureSizeY + sizey) / 2f);
        }


        /// <summary>
        /// Recursively returns all hard-referenced, nested objects which need to be registered. Implemented for DB loading.
        /// </summary>
        /// <returns></returns>
        public virtual ICollection<IHasGalaxyID> GetRegisterableNestedObjects()
        {
            return new List<IHasGalaxyID> { this };
        }

        public virtual IDBObject GetDBObject()
        {
            return _model.GetClone();
        }
    }

    public interface IConstructionStructureModel : IStructureModel
    {
        StructureTypes FinishedType { get;}
        float CurrentConstructionPoints { get;}
        float FullConstructionPoints { get; }
        float ConstructionRate { get; }

    
    }

    public interface IGenerator
    {
        float Generate(float elapsedMS);
    }

    public class SupplyDrone
    {

        protected IResourceSink _destinationStructure;

        protected IResourceSupply _parentStructure;

        protected float _distanceToDestination;

        /// <summary>
        /// Units
        /// </summary>
        protected float _capacity = 10;
        protected int _travelSpeed = 1;

        [Key]
        public int DroneID { get; set; }

        public ResourceTypes ResourceType { get; set; }

        public int? DestinationID { get; set; }

        public int? ParentStructureID { get; set; }

        public float SupplyRate { get; set; }

        public SupplyDrone(IResourceSupply parentStructure, int id)
        {
            _parentStructure = parentStructure;
            SupplyRate = 0;
            DroneID = id;
        }

        /// <summary>
        /// Also sets the supply rate
        /// </summary>
        /// <param name="destination"></param>
        public void SetDestination(IStructure destination)
        {
            DestinationID = destination.Id;
            _distanceToDestination = _parentStructure.DistanceTo(destination);

            SupplyRate = _capacity * _travelSpeed / _distanceToDestination / 2;//Factor of 2 to account for return travel
        }

        public void SupplyDestination(double elapsedMs)
        {
            //float supplyAmount = SupplyRate * (float)elapsedMs / 3600000f;//Convert ms to hours
            //if(_parentStructure.GetResourceAmount(ResourceType) >= supplyAmount && _destinationStructure.GetAvailableCapacity(ResourceType) >= supplyAmount)
            //{
            //    _parentStructure.RemoveResources(ResourceType, supplyAmount);
            //    _destinationStructure.StoreResources(ResourceType, ref supplyAmount);
            //}

        }


    }

    

    

    
}