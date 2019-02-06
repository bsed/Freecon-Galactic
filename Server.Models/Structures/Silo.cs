using Freecon.Models.TypeEnums;
using System.Collections.Generic;
using Core.Models.Enums;
using Server.Models.Extensions;
using Server.Managers;

namespace Server.Models.Structures
{
    public class Silo : ResourceStructure<SiloModel, StructureStats>, IResourceSupply, IResourceSink
    {

        //Stores resources
        public bool IsFull { get { return _model.IsFull; } set { _model.IsFull = value; } }
        public float Capacity { get { return _model.Capacity; } set { _model.Capacity = value; } }
        public float UnitsStored { get { return _model.UnitsStored; } set { _model.UnitsStored = value; } }

        public int MaxDrones { get { return _model.MaxDrones; } set { _model.MaxDrones = value; } }
        public int NumDrones { get { return _model.NumDrones; } set { _model.NumDrones = value; } }
        public int DroneLevel { get { return _model.DroneLevel; } set { _model.DroneLevel = value; } }
        public float SupplyRatePerDrone { get { return _model.SupplyRatePerDrone; } set { _model.SupplyRatePerDrone = value; } }

        /// <summary>
        /// Represents the thorium rate of production
        /// </summary>
        public float ThoriumRate { get { return _model.Resources[ResourceTypes.Thorium].CurrentRate; } set { _model.Resources[ResourceTypes.Thorium].CurrentRate = value; } }

        public Silo()
        {
            _model.Stats.StructureType = StructureTypes.Silo;
        }

        public Silo(SiloModel s):base(s)
        {
            _model = s;
            _model.Stats.StructureType = StructureTypes.Silo;
        }

        public Silo(float posX, float posY, int galaxyID, int ownerID, int currentAreaID)
            : base(posX, posY, galaxyID, ownerID, currentAreaID)
        {
            
            _model.Stats.StructureType = StructureTypes.Silo;            

            _model.Capacity = 1000;
            _model.UnitsStored = 0;
            _model.MaxDrones = 5;
            _model.NumDrones = 1;
            _model.DroneLevel = 1;
            _model.SupplyRatePerDrone = 100;
            

            _model.Resources = new Dictionary<ResourceTypes, Resource>();
            _model.Resources.Add(ResourceTypes.Bauxite, new Bauxite());
            _model.Resources.Add(ResourceTypes.Hydrogen, new Hydrogen());
            _model.Resources.Add(ResourceTypes.IronOre, new IronOre());
            _model.Resources.Add(ResourceTypes.Organics, new Organics());
            _model.Resources.Add(ResourceTypes.Silica, new Silica());
            _model.Resources.Add(ResourceTypes.ThoriumOre, new ThoriumOre());
            _model.Resources.Add(ResourceTypes.Medicine, new Medicine());
            _model.Resources.Add(ResourceTypes.Hydrocarbons, new Hydrocarbons());

            _model.Drones = new List<SupplyDrone>();
            _model.Drones.Add(new SupplyDrone(this, 1));
        }

        public float GetSupplyRate(IResourceSink s)
        {
            float distance = this.DistanceTo(s);

            //Can tweak later, quick and dirty inverse linear function for now
            if (distance > 10)
                return 1f / 10f * _model.SupplyRatePerDrone;
            else if (distance <= 1)
                return .1f * _model.SupplyRatePerDrone;
            else                
                return (-.09f * distance) * _model.SupplyRatePerDrone;
        }

      

        /// <summary>
        /// Sets all resource rates to 0
        /// </summary>
        public void ResetResourceRates()
        {
            foreach (Resource r in _model.Resources.Values)
            {
                r.CurrentRate = 0;
            }
        }

        /// <summary>
        /// Adds a drone. Returns non-zero drone ID if add was sucessful, false otherwise
        /// </summary>
        /// <returns></returns>
        public int AddDrone()
        {
            if (_model.Drones.Count == MaxDrones)
                return 0;

            Stack<int> usedIDs = new Stack<int>();
            foreach (SupplyDrone d in _model.Drones)
                usedIDs.Push(d.DroneID);

            int nextID = (int)_model.Drones.Count;
            while (usedIDs.Contains(nextID))
                nextID--;

            _model.Drones.Add(new SupplyDrone(this, nextID));
            return nextID;
        }

        public void RemoveDrone(int ID)
        {
            _model.Drones.Remove(_model.Drones.Find(d => { return d.DroneID == ID; }));
        }

        public void SupplyUpdate(float elapsedMS, ICollection<IResourceSink> resourceSinks, float multiplier)
        {
            if (!_model.Enabled)
                return;

            //Maximum supply that this silo can provide in this update. When it reaches 0, return.
            float supplyAvailableThisUpdate = _model.NumDrones * _model.SupplyRatePerDrone * elapsedMS * multiplier;            

            //Start with PowerPlanets, supplying thorium
            foreach(IResourceSink pp in resourceSinks)
            {
                if(pp is PowerPlant)
                {
                    _supplyResource(pp, ResourceTypes.ThoriumOre, ref supplyAvailableThisUpdate, elapsedMS);
                }          

                    if (supplyAvailableThisUpdate <= 0)//If we've supplied all that we can
                        return;                
            }

            //Supply other structures
            foreach(IResourceSink s in resourceSinks)
            {
                if (s is PowerPlant)
                    continue;

                foreach(ResourceTypes r in s.ConsumedResources)
                {
                    _supplyResource(s, r, ref supplyAvailableThisUpdate, elapsedMS);

                    if (supplyAvailableThisUpdate <= 0)//If we've supplied all that we can
                        return;

                }

            }        
        }

        /// <summary>
        /// Supplies the given IResourceSink with the given resource type.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="type"></param>
        /// <param name="supplyAvailableThisUpdate"></param>
        /// <param name="elapsedMS"></param>
        void _supplyResource(IResourceSink s, ResourceTypes type, ref float supplyAvailableThisUpdate, float elapsedMS)
        {          
            
            float supplyAmount = GetSupplyRate(s) * elapsedMS;
            //Set proper supply amount, according to the thorium supply available
            if (supplyAmount > _model.Resources[type].NumUnits)
            {
                supplyAmount = _model.Resources[type].NumUnits;
            }

            //Make sure supplyAmount doesn't exceed how much can be supplied this update
            if (supplyAmount > supplyAvailableThisUpdate)
            {
                supplyAmount = supplyAvailableThisUpdate;
            }

            float amountUnstored = supplyAmount;
            s.StoreResources(type, ref amountUnstored);
            supplyAvailableThisUpdate -= (supplyAmount - amountUnstored);
            _model.Resources[type].TakeResource(supplyAmount - amountUnstored);

            if (_model.Resources[type].NumUnits <= 0)//If we've supplied all the resources that we can
            {
#if DEBUG
                if (_model.Resources[type].NumUnits < 0)
                    ConsoleManager.WriteLine("Remaining resource amount is less than 0", ConsoleMessageType.Warning);//This shouldn't happen, but just in case
#endif
                return;

            }

            

        }
    

    }
    
    public class SiloModel : ResourceStructureModel<StructureStats>
    {
       

        public int MaxDrones { get; set; }
        public int NumDrones { get; set; }
        public int DroneLevel { get; set; }
        public float SupplyRatePerDrone { get; set; }//Rate per millisecond

        public List<SupplyDrone> Drones { get; set; }             

        public SiloModel()
        {
            StructureType = StructureTypes.Silo;
        }

        public SiloModel(SiloModel m):base(m)
        {
            IsFull = m.IsFull;
            Capacity = m.Capacity;
            UnitsStored = m.UnitsStored;
            MaxDrones = m.MaxDrones;
            NumDrones = m.NumDrones;
            DroneLevel = m.DroneLevel;
            SupplyRatePerDrone = m.SupplyRatePerDrone;
            Drones = m.Drones;

        }


    }
}
