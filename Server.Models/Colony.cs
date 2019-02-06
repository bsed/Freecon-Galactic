using Server.Managers;
using System.Collections.Generic;
using Lidgren.Network;
using SRServer.Services;
using Server.Interfaces;
using Server.Models.Structures;
using Freecon.Core.Networking.Models;
using Server.Models.Research;
using System.Linq;
using Freecon.Models.TypeEnums;
using Core.Models.Enums;
using Freecon.Models;
using Core.Models;
using Freecon.Core.Networking.Models.Messages;
using Freecon.Core.Interfaces;

namespace Server.Models
{
    public class Colony : Area<ColonyModel>
    {        
        private List<Refinery> refineries;
        public List<IResourceSupply> _resourceSuppliers = new List<IResourceSupply>();
        public List<IResourceSink> _resourceSinks = new List<IResourceSink>();


        private List<MineStructure> mines;
        private List<Biodome> biodomes;
        private List<PowerPlant> powerPlants;
        public CommandCenter CommandCenter { get { return _commandCenter; } protected set { _commandCenter = value; _model.CommandCenterID = value.Id; } }
        CommandCenter _commandCenter;
        public float TotalPopulation { get { return _model.TotalPopulation; } set { _model.TotalPopulation = value; }}
        public float MaxPopulation { get { return _model.MaxPopulation; } set { _model.MaxPopulation = value; }}
        public float MaxPowerAvailable{get { return _model.MaxPowerAvailable; }set { _model.MaxPowerAvailable = value; }}//Needs to be updated when powerplants are added/removed

        bool _useStoredThoriumForPower = false;

        public bool DisableUpdates = true;
        public bool IsUpdating { get; protected set; }

        public Warphole WarpholeToPlanet { get { return Warpholes[0]; } }
        public Warphole WarpholeToSpace { get { return Warpholes[1]; } }

        public bool PropagandaDiscovered { get { return _model.ResearchHandler.PropagandaDiscovered; } }

        public int SystemID { get { return _model.SystemID; } set { _model.SystemID = value; } }

        public int? OwnerDefaultTeamID { get { return _model.OwnerTeamID; } set { _model.OwnerTeamID = value; } }

        public int? OwnerID { get { return _model.OwnerID; } set { _model.OwnerID = value; } }

        public float Cash { get { return _model.Cash; } }
        
        public string Name
        {
            get { return _model.Name; }
            set { _model.Name = value; }
        }

        object _UPDATELOCK = new object();

        public Dictionary<SliderTypes, Slider> Sliders { get { return _model.Sliders; } }
        
        protected Colony() 
        {
            _structures = new Dictionary<int, IStructure>();
        }

        /// <summary>
        /// Creates a new colony.
        /// </summary>
        public Colony(int ID, Player owner, IArea parentArea, LocatorService ls)
            : base(ID, ls)
        {
            refineries = new List<Refinery>();
            mines = new List<MineStructure>();
            biodomes = new List<Biodome>();
            powerPlants = new List<PowerPlant>();
            _resourceSuppliers = new List<IResourceSupply>();


            OwnerID = owner.Id;
            OwnerDefaultTeamID = owner.DefaultTeamID;

            Warpholes = new List<Warphole>();

            // Warp to planet surface
            Warpholes.Add(new Warphole(432342, 35234, ID, parentArea.Id, 0));

            // Warp to space
            Warpholes.Add(new Warphole(62342, 42356, ID, (int)parentArea.ParentAreaID, 1));
            SystemID = (int)parentArea.ParentAreaID;
            

            ParentAreaID = parentArea.Id;

            TotalPopulation = 100;
            MaxPopulation = 500;
            MaxPowerAvailable = 50;//Needs to be updated when powerplants are added/removed
            _model.NewsEvents = new Queue<ColonyNewsEvent>();
            
            

            _model.ResearchHandler = new ResearchHandler();
            _model.OwnerName = owner.Username;

            _structures = new Dictionary<int, IStructure>();
        }


        public Colony(ColonyModel c, LocatorService ls):base(c, ls)
        {
            _model = c;
            refineries = new List<Refinery>();
            mines = new List<MineStructure>();
            biodomes = new List<Biodome>();
            powerPlants = new List<PowerPlant>();
            _resourceSuppliers = new List<IResourceSupply>();

            _model.ResearchHandler.ResetBonuses();

            
        }

        public void ChangeOwner(Player newOwner)
        {
            CommandCenter.OwnerID = newOwner.Id;
            CommandCenter.OwnerTeamID = newOwner.DefaultTeamID;
            newOwner.ColonizedPlanetIDs.Add((int)this.ParentAreaID);

            _model.OwnerName = newOwner.Username;

            OwnerID = newOwner.Id;
            OwnerDefaultTeamID = newOwner.DefaultTeamID;
        }

        public override void SendEntryData(HumanPlayer sendHere, bool warping, IShip playerShip)
        {
            sendHere.SendMessage(new NetworkMessageContainer(new MessageEmptyMessage(), MessageTypes.EnterColony));

        }

        /// <summary>
        /// Checks that there is enough population and power to run all buildings, disables buildings as necessary
        /// </summary>
        /// <param name="elapsedMS"></param>
        public override void Update(float currentTime)
        {
            if (DisableUpdates)
                return;

            lock (_UPDATELOCK)
            {
                _lastUpdateTime = currentTime;
                IsUpdating = true;
                HashSet<ProblemFlagTypes> newProblemFlags = new HashSet<ProblemFlagTypes>();

                float elapsedMS = currentTime - _lastUpdateTime;
                //DO NOT CALL STRUCTURE UPDATES HERE, StructureManager HANDLES STRUCTURE UPDATES
                TotalPopulation = _countPopulation();
                _model.PopulationRate = _countPopulationRate();
                _model.MoraleAvg = _countMorale();
                _model.MoraleRateAvg = _countMoraleRate();

                CheckPopulationAvailability();//Make sure all buildings have enough population to run

                //Supply this update, assume there is enough power for currently enabled buildings to run this update, small numerical error if they don't
                foreach (IResourceSupply s in _resourceSuppliers)
                {
                    if (s.Enabled)
                    {
                        if (s is MineStructure)
                        {
                            s.SupplyUpdate(elapsedMS, _resourceSinks, Sliders[SliderTypes.Mining].CurrentValue);
                        }
                        else
                        {
                            s.SupplyUpdate(elapsedMS, _resourceSinks, 1);
                        }
                    }
                }

                //Generate power
                float powerAvailableThisUpdate = CommandCenter.Generate(elapsedMS);
                foreach (PowerPlant p in powerPlants)
                    powerAvailableThisUpdate += p.Generate(elapsedMS);

                powerAvailableThisUpdate *= _model.ResearchHandler.PowerGenerationMultiplier;

                //Disable buildings as appropriate
                if (!CheckPowerAvailability(powerAvailableThisUpdate, elapsedMS))
                    newProblemFlags.Add(ProblemFlagTypes.NotEnoughPower);

                if (_model.ResearchHandler.Update(elapsedMS, TotalPopulation, 0, Sliders[SliderTypes.Research].CurrentValue) != null)//If research has been discovered...
                {
                    _resetResearchBonuses();
                }

                //Cash
                _model.CashRate = _model.Sliders[SliderTypes.TaxRate].CurrentValue * _model.TaxPerColonistMs * _model.ResearchHandler.TaxRevenueMultiplier;
                float taxCash = _model.CashRate * elapsedMS;
                DepositCash(taxCash);

                //Update problem flags
                for(int i = 0; i < _structures.Keys.Count; i++)
                {
                    foreach (ProblemFlagTypes pi in _structures.ElementAt(i).Value.ProblemFlags)
                    {
                        newProblemFlags.Add(pi);
                    }
                }
                _model.ProblemFlags = newProblemFlags;
                IsUpdating = false;
            }

        }
        
        void _resetResearchBonuses()
        {
            foreach(Biodome b in biodomes)
            {
                b.PopResearchMultiplier = _model.ResearchHandler.GrowthRateMultiplier;
                b.MoraleRateMultiplier = _model.ResearchHandler.MoraleRateMultiplier;
            }
            for (int i = 0; i < _structures.Count; i++)
            {
                var s = _structures.ElementAt(i);
                s.Value.DamageMultiplier = _model.ResearchHandler.StructureDamageMultiplier;

                if (s.Value is Factory)
                {
                    ((Factory)s.Value).ConstructionRateMultiplier = 0;
                }

            }

        }

        public override void AddStructure(IStructure s)
        {
            base.AddStructure(s);
            
            RegisterStructure(s);
        }

        /// <summary>
        /// Returns the sum of populations of each biodome.
        /// </summary>
        /// <returns></returns>
        float _countPopulation()
        {
            float total = 0;
            foreach(Biodome b in biodomes)
            {
                total += b.Population;
            }
            return total;
        }

        float _countPopulationRate()
        {
            float total = 0;
            foreach(Biodome b in biodomes)
            {
                total += b.PopulationRate;
            }
            return total;
        }

        /// <summary>
        /// Returns a simple average of all biodomes. Not sure if we want to allow different biodomes to have different morale
        /// </summary>
        /// <returns></returns>
        float _countMorale()
        {
            float avg = 0;
            foreach(Biodome b in biodomes)
            {
                avg += b.Morale;
            }
            return avg / biodomes.Count;
        }

        float _countMoraleRate()
        {
            float avg = 0;
            foreach (Biodome b in biodomes)
            {
                avg += b.MoraleRate;
            }
            return avg / biodomes.Count;
        }

        public void WriteStructuresToMessage(NetOutgoingMessage msg)
        {



        }

        public void RegisterStructure(IStructure s)
        {
            lock (_UPDATELOCK)
            {
                bool isRegistered = false;

                if (s.StructureType == StructureTypes.Mine)
                {
                    mines.Add((MineStructure)s);
                    ((MineStructure)s).ProductionBonuses = _model.ResearchHandler.ProductionBonuses;
                    isRegistered = true;
                }
                if (s.StructureType == StructureTypes.PowerPlant)
                {
                    powerPlants.Add((PowerPlant)s);
                    isRegistered = true;
                }
                if (s.StructureType == StructureTypes.Biodome)
                {
                    biodomes.Add((Biodome)s);
                    isRegistered = true;
                    TotalPopulation = _countPopulation();
                }
                if (s is IResourceSupply)
                {
                    _resourceSuppliers.Add((IResourceSupply)s);
                    isRegistered = true;
                }
                if (s is IResourceSink)
                {
                    _resourceSinks.Add((IResourceSink)s);
                    isRegistered = true;
                }
                if (s.StructureType == StructureTypes.Refinery)
                {
                    refineries.Add((Refinery)s);
                    isRegistered = true;
                }

                if (s.StructureType == StructureTypes.CommandCenter)
                {
                    CommandCenter = (CommandCenter)s;
                    isRegistered = true;
                }

                if (!isRegistered)
                    ConsoleManager.WriteLine("Warning, structure could not be registered with the colony. Did someone forget to add it to Colony.RegisterStructure()?", ConsoleMessageType.Warning);
            }

        }

        public int GetStructureCount()
        {
            return (int)GetParentArea().GetStructures().Count;
        }

        /// <summary>
        /// Returns amount if Cash <= amount, withdraws and returns as much cash as possible otherwise
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public float WithdrawCash(float amount)
        {
            float retval = _model.Cash;

            if (amount <= _model.Cash)
            {
                _model.Cash -= amount;
                retval = amount;
            }
            else
            {
                _model.Cash = 0;
            }

            return retval;
        }

        /// <summary>
        /// Returns undeposited amount
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public float DepositCash(float amount)
        {
            float retval;
            float spaceAvailable = _model.MaxCash - _model.Cash;

            if (amount > spaceAvailable)
            {
                retval = amount - spaceAvailable;
                _model.Cash = _model.MaxCash;
            }
            else
            {
                retval = 0;
                _model.Cash += amount;
            }
        
            return retval;
            
        }

        #region Structure Updates
        
        /// <summary>
        /// Checks to make sure that all structures have enough people to operate, disables structures when available population is exhausted
        /// </summary>
        public void CheckPopulationAvailability()
        {
            int popAvailable = (int)TotalPopulation;//Truncation is fine

            foreach (IStructure s in GetParentArea().GetStructures().Values)
            {
                if (!s.Enabled)
                    continue;
                else if (s.PeopleRequired <= (popAvailable * _model.ResearchHandler.PopulationUsageMultiplier))
                    popAvailable -= s.PeopleRequired;
                else
                {
                    s.Disable("Insufficient population to operate this structure.");
                    _AddToNewsQueue(new ColonyNewsEvent() { Color = "Red", Message = "Structure disabled: not enough colonists." });
                }
            }


        }

        /// <summary>
        /// Checks to make sure that all structures have enough people to operate, disables structures when available power is exhausted. Returns true if there is enough power, false otherwise
        /// </summary>
        public bool CheckPowerAvailability(float powerAvailable, float elapsedMS)
        {
            float powerAvailableTemp = powerAvailable;
            bool enoughPower = true;
            foreach (IStructure s in GetParentArea().GetStructures().Values)
            {
                float powerConsumedThisUpdate = s.PowerConsumptionRate * elapsedMS;

                if (!s.Enabled)
                    continue;
                else if (powerConsumedThisUpdate <= powerAvailable)
                    powerAvailable -= (powerConsumedThisUpdate);
                else
                {
                    s.Disable("YOU MUST CONSTRUCT ADDITIONAL PYLONS. [Not enough power]");
                    _AddToNewsQueue(new ColonyNewsEvent() { Color = "Red", Message = "Structure disabled: not enough power." });
                    enoughPower = false;
                }
            }
            return enoughPower;
        }
        

        #endregion

        void _AddToNewsQueue(ColonyNewsEvent ev)
        {
            //Only save the last x news messages
            if(_model.NewsEvents.Count > 20)
            {
                _model.NewsEvents.Dequeue();
                _model.NewsEvents.Enqueue(ev);
            }

        }

        public override bool CanAddStructure(Player player, StructureTypes buildingType, float xPos, float yPos, out string resultMessage)
        {
            resultMessage = "You can't deploy a structure in a colony, you'll kill somebody! Are you insane?";
            return false;
        }


        #region Moving Players and Ships
        public virtual void addShip(IShip s)
        {
            _model.ShipIDs.Add(s.Id);
            _shipCache.Add(s.Id, s);
        }

        public override void AddShip(NPCShip npc, bool suspendNetworking)
        {
            _model.ShipIDs.Add(npc.Id);
            _shipCache.Add(npc.Id, npc);
        }

        public override void RemoveShip(IShip s)
        {
            _model.ShipIDs.Remove(s.Id);
            _shipCache.Remove(s.Id);
        }

        public override void RemoveShip(NPCShip npc)
        {
            _model.ShipIDs.Remove(npc.Id);
            _shipCache.Remove(npc.Id);
        }

        public override void MoveShipHere(IShip s)
        {
            if (s.GetArea() != null)
                s.GetArea().RemoveShip(s);

            addShip(s);
        }

        public override void MoveShipHere(NPCShip npc)
        {
            if (npc.GetArea() != null)
                npc.GetArea().RemoveShip(npc);

            addShip(npc);
        }

        /// <summary>
        /// Moves the player, sets simulating player in old and new systems appropriately.
        /// </summary>
        /// <param name="p"></param>
        public override void MovePlayerHere(Player p, bool isWarping)
        {
            if (p.CurrentAreaID != null)
                p.GetArea().RemovePlayer(p); //Removes player from his current (old in this context) area
            AddPlayer(p, isWarping); //Adds player to this area
        }
  
        #endregion
       
        public override IDBObject GetDBObject()
        {
            return _model.GetClone();         
  
        }
    }

    public class Colony_HoldingsVM
    {
        public float Cash { get; set; }

        public string Name { get; set; }

        public string OwnerName { get; set; }

        public float PowerInUse { get; set; }

        public int SystemID { get; set; }

        public float TaxRate { get; set; }

        public float TotalPopulation { get; set; }

        public float TotalPowerAvailable { get; set; }


    }
}