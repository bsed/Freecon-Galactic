using System;
using System.Collections.Generic;
using Freecon.Models.TypeEnums;
using Freecon.Core.Networking.Models;
using SRServer.Services;
using Server.Models.Mathematics;
using Server.Interfaces;
using Core.Models;
using Server.Models.Interfaces;
using Core.Models.CargoHandlers;
using Core.Models.Enums;
using Freecon.Core.Interfaces;
using Freecon.Core.Networking.Models.Messages;
using Server.Managers;
using System.Linq;
using Freecon.Core.Networking.Models.Objects;

namespace Server.Models
{
    public class Port : Area<PortModel>, ISerializable, IHasCargo
    {
        public int CashFromSales = 0;

        public bool IsMoon;

        public IReadOnlyDictionary<PortServiceTypes, PortService> ServicesForSale {get{return _servicesForSale;}}
        Dictionary<PortServiceTypes, PortService> _servicesForSale = new Dictionary<PortServiceTypes, PortService>(5);//Services the port is selling
        
        public int Size;

        public CargoHandlerPort_ROVM Cargo { get; protected set; }

        HashSet<ShipTypes> ShipsForSale;

        protected Port() { }

        public Port(PortModel p, LocatorService ls)
            : base(p, ls)
        {
           
            Cargo = new CargoHandlerPort_ROVM(p.Cargo);
            
            
            CashFromSales = p.CashFromSales;               

            _servicesForSale = new Dictionary<PortServiceTypes, PortService>();
            foreach(var o in p.ServicesForSale)
            {
                _servicesForSale.Add(o.OutfitType, o);
            }
            
            IsMoon = p.IsMoon;
           
            Size = p.Size;
        }

        public Port(int ID, Planet moon, string portName, List<ShipStats> allShipStats, LocatorService ls)
            : base(ID, ls)
        {
            var r = new Random(6546345);
            Size = r.Next(1, 3);

            Distance = moon.Distance;
            MaxTrip = moon.MaxTrip;
            CurrentTrip = moon.CurrentTrip;
            IDToOrbit = moon.IDToOrbit;
            IsMoon = true;

            ParentAreaID = moon.ParentAreaID;
            AreaName = portName;

            _servicesForSale.Add(PortServiceTypes.HullRepair, new ShipRepair(this));
            Cargo = new CargoHandlerPort_ROVM(_model.Cargo);
           
        }


        public override void SendEntryData(HumanPlayer sendHere, bool warping, IShip playerShip)
        {
            var portEntryData = new PortEntryData(base.GetEntryData(sendHere.ActiveShipId, true, true))
            {
                Id = Id,
                AreaName = AreaName,
                AreaSize = AreaSize
            };

            portEntryData.NewPlayerXPos = playerShip.PosX;
            portEntryData.NewPlayerYPos = playerShip.PosY;

            sendHere.SendMessage(portEntryData, MessageTypes.PortDockApproval);
        }
           
        /// <summary>
        /// Callback given to CargoHandler to allow for the port to set the object's price
        /// </summary>
        /// <param name="c"></param>
        public float PriceGetter(StatefulCargo_RO c, int numInStock, PriceType p)
        {            
            return 666;
        }

        /// <summary>
        /// Callback given to CargoHandler to allow for the port to set the object's price
        /// </summary>
        /// <param name="c"></param>
        public float PriceGetter(StatelessCargo c, PriceType p)
        {
            return 666;
        }
        
        public CargoHandler_ReadOnlyVM<CargoHandlerModel> GetCargo()
        {
            return Cargo;
        }

        /// <summary>
        /// If amountRequested is greater than amount allowed (e.g. more health requested than MaxHealth), returns amount corresponding to max allowed
        /// </summary>
        /// <param name="ship"></param>
        /// <param name="serviceType"></param>
        /// <param name="amountAllowed"></param>
        /// <returns></returns>
        public float GetTotalServicePrice(PortServiceRequestData serviceData)
        {
            if(!_servicesForSale.ContainsKey(serviceData.ServiceType))//Ideally we'll never get here
            {
                return float.MaxValue;
            }

            return _servicesForSale[serviceData.ServiceType].GetTotalPrice(serviceData);
        }

        /// <summary>
        /// Checks if a service request is valid. Sets serviceRequestData.TotalPrice
        /// </summary>
        /// <param name="serviceRequestData"></param>
        /// <returns></returns>
        public PurchaseResult ValidatePurchaseResult(PortServiceRequestData serviceRequestData)
        {
         
            var ship = serviceRequestData.ReceivingShip;
            
            if(!ServicesForSale.ContainsKey(serviceRequestData.ServiceType))//Make sure port offers this service
            {
                return PurchaseResult.ServiceNotAvailable;
            }
            else if (Id != ship.CurrentAreaId)//Make sure the IShip is actually docked
            {    
                return PurchaseResult.WrongArea;
            }
            else if (!ShipIDs.Contains(ship.Id))
            {
                ConsoleManager.WriteLine("Inconsistent state between Ship.CurrentAreaID and port.ShipIDs", ConsoleMessageType.Error);
                return PurchaseResult.WrongArea;
            }
            else
            {
                return _servicesForSale[serviceRequestData.ServiceType].ValidateRequest(serviceRequestData);
            }
        }



        public void ExecuteService(object sender, ITransactionRemoveStatelessCargo transaction)          
        {
            var serviceRequestData = transaction.OnCompletionData as PortServiceRequestData;

            _servicesForSale[serviceRequestData.ServiceType].Execute(serviceRequestData);          

        }
        public override bool CanAddStructure(Player player, StructureTypes buildingType, float xPos, float yPos, out string resultMessage)
        {
            resultMessage = "You can't deploy a structure in a port, you'll kill somebody! Are you insane?";
            return false;
        }

        public override IDBObject GetDBObject()
        {
            return _model.GetClone();
        }

        #region Moving Players

        public override void MovePlayerHere(Player p, bool isWarping)
        {
            if (p.CurrentAreaID != null)
                p.GetArea().RemovePlayer(p); //Removes player from his current (old in this context) area
            AddPlayer(p, isWarping); //Adds player to this area
        }

        public override void RemovePlayer(Player p)
        {
            lock (PLAYERLOCK)
            {
                _onlinePlayerIDs.Remove(p.Id); //Remove player system
                _onlinePlayerCache.Remove(p.Id);
            }
           
        }

        public override void AddPlayer(Player p, bool isWarping)
        {
            lock (PLAYERLOCK)
            {
                if (p.IsOnline)
                {
                    if (p.PlayerType == PlayerTypes.Human)
                    {
                        _humanPlayerIDs.Add(p.Id);
                    }
                    else if (p.PlayerType == PlayerTypes.NPC)
                    {
                        _NPCPlayerIDs.Add(p.Id);
                    }

                    _onlinePlayerIDs.Add(p.Id);
                    _onlinePlayerCache.Add(p.Id, p);

                    //Sends info about system to player, tells player to "move" to this system
                }
            }
        }

        #endregion

        #region Moving Ships

        public override void AddShip(IShip s, bool suspendNetworking)
        {

            _model.ShipIDs.Add(s.Id);
            _shipCache.Add(s.Id, s);

            #region Send RecieveNewShip message to all clients in new area

            if (_onlinePlayerIDs.Count >= 1 && !suspendNetworking)
            {

                var messageData = new MessageReceiveNewPortShip();
                messageData.Id = s.Id;
                messageData.Username = s.GetPlayer().Username;
                messageData.PortID = Id;
              
                BroadcastMessage(new NetworkMessageContainer(messageData, MessageTypes.ReceiveNewPortShip), s.PlayerID);
                
            }

            #endregion
        }

        public override void RemoveShip(IShip s)
        {
            base.RemoveShip(s);
            SetShipPosition(s);
        }

        public void SetShipPosition(IShip s)
        {
            float tempx = 0;
            float tempy = 0;

            SpatialOperations.GetRandomPointInRadius(ref tempx, ref tempy, Size, Size + 1);

            s.PosX = tempx;
            s.PosY = tempy;
            s.VelX = 0;
            s.VelY = 0;
        }

        #endregion

        #region Moving NPCs

        public override void MoveShipHere(NPCShip npc)
        {
            if (npc.CurrentAreaId != null)
                npc.GetArea().RemoveShip(npc);

            AddShip(npc);
        }

        public override void RemoveShip(NPCShip npc)
        {
            _model.ShipIDs.Remove(npc.Id);
            _shipCache.Remove(npc.Id);
            SetShipPosition(npc);

        }

        public override void AddShip(NPCShip npc, bool suspendNetworking=false)
        {
            _model.ShipIDs.Add(npc.Id);
            _shipCache.Add(npc.Id, npc);

        }

        #endregion
             
    }

    #region Services

    /// <summary>
    /// Base class for services provided at ports (such as IShip repair)
    /// </summary>
    public abstract class PortService
    {
        public int Id { get; set; }

        public float CurrentPrice;
        public string Name;
        public PortServiceTypes OutfitType;

        protected Port port;

        public PortService() { }

        public PortService(Port p)
        {
            port = p;
        }

        /// <summary>
        /// May modify serviceRequestData.NumUnits
        /// </summary>
        /// <param name="serviceRequestData"></param>
        /// <returns></returns>
        public virtual PurchaseResult ValidateRequest(PortServiceRequestData serviceRequestData)
        {
            var ship = serviceRequestData.ReceivingShip;            
           
            if(GetNumAllowed(serviceRequestData) < serviceRequestData.NumUnits)
                return PurchaseResult.TooManyUnitsRequested;

            serviceRequestData.TotalPrice = port.GetTotalServicePrice(serviceRequestData);

            if (ship.Cargo.GetCargoAmount(StatelessCargoTypes.Cash) < serviceRequestData.TotalPrice)
            {
                return PurchaseResult.NotEnoughCash;
            }

            return PurchaseResult.Success;
        }

        /// <summary>
        /// Should return true upon succesful completion.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public abstract bool Execute(PortServiceRequestData requestData);

        /// <summary>
        /// Sets current price based on current server price, num in inventory, anything else applicable
        /// </summary>
        public abstract void setCurrentPrice();

        public abstract float GetTotalPrice(PortServiceRequestData serviceRequestData);

        public abstract float GetNumAllowed(PortServiceRequestData serviceRequestData);

      
    }

    public class ShipRepair : PortService
    {
        public ShipRepair() { }

        public ShipRepair(Port p) : base(p)
        {
            Name = "Hull Repair";
            OutfitType = (byte) PortServiceTypes.HullRepair;
            setCurrentPrice();
        }

        public override void setCurrentPrice()
        {
            CurrentPrice = 10; //Credits per health point
        }

        public override bool Execute(PortServiceRequestData serviceRequestData)
        {
            serviceRequestData.ReceivingShip.CurrentHealth += serviceRequestData.NumUnits;
            if (serviceRequestData.ReceivingShip.CurrentHealth > serviceRequestData.ReceivingShip.MaxHealth)
            {
                serviceRequestData.ReceivingShip.CurrentHealth = serviceRequestData.ReceivingShip.MaxHealth;
            }
            return true;
        }


        public override float GetNumAllowed(PortServiceRequestData serviceRequestData)
        {
            return serviceRequestData.ReceivingShip.MaxHealth - serviceRequestData.ReceivingShip.CurrentHealth;
        }

        public override float GetTotalPrice(PortServiceRequestData serviceRequestData)
        {
            return serviceRequestData.NumUnits * CurrentPrice;
        }
               
    }


    /// <summary>
    /// Large glob of all possible data that a service might need to execute
    /// Might break this up later
    /// </summary>
    public class PortServiceRequestData:TradeRecordModel
    {
        public readonly PortServiceTypes ServiceType;

        public readonly IShip ReceivingShip;

        public readonly float NumUnits;


        /// <summary>
        /// Set by port during validation
        /// </summary>
        public float TotalPrice;
        
    }

    public enum PurchaseResult:byte
    {
        Success,
        
        NotEnoughCash,
        NotEnoughCargoSpace,

        CargoOutOfStock,//Port doesn't have cargo in stock. Shouldn't happen. Probably a bug or hacking
        CargoNotInHolds,//Ship doesn't have the cargo it's trying to sell. Shouldn't happen. Probably a bug or hacking

        WrongArea,//Hacking or a bug?

        ShipNotAvailable,//Port doesn't have this ship

        ServiceNotAvailable,//Port doesn't offer this service
        ServiceNotImplemented,

        TooManyUnitsRequested,
        NoItemsSpecified,//Empty purchase request. Should be validated clientside first, but just in case...

        FailureReasonUnknown,//I'm a lazy asshole


    }

    #endregion

    
   
}