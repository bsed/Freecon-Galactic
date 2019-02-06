using System;
using System.Collections.Generic;
using Freecon.Models.TypeEnums;
using Server.Models;
using SRServer.Services;
using Server.Models.Economy;
using Server.Managers.Synchronizers.Transactions;
using System.Threading.Tasks;
using Core.Models.Enums;
using Core.Models.CargoHandlers;
using Server.Interfaces;
using Freecon.Models;
using Server.Database;
using System.Linq;
using Core.Models;
using System.Collections.Concurrent;
using Core;
using Freecon.Core.Utils;
using Freecon.Core.Interfaces;
using Freecon.Core.Models.Enums;
using Freecon.Core.Networking.Objects;
using Freecon.Core.Networking.Models;
using Freecon.Core.Networking.Models.Messages;
using System.Timers;
using Server.Models.Interfaces;


namespace Server.Managers.Economy
{


    /// <summary>
    /// Handles all monetary aspects of all goods and services
    /// //TODO: Probably need to convert this to a synchronizer type architecture, or come up with some other way to remove all the locking
    /// </summary>
    public class EconomyManager: ITradeTerminator
    {
        ShipManager _shipManager;
        IPlayerLocator _playerLocator;
        IAreaLocator _areaLocator;
        CargoSynchronizer _cargoSynchronizer;
        IDatabaseManager _databaseManager;
        ISlaveIDProvider _slaveIDProvider;

        object TRADELOCK = new object();


        ConcurrentDictionary<int, ShipShipTrade> _shipShipTrades;
        ConcurrentDictionary<int, int> _shipIDToTradeID;

        HashSet<int> _tradingPlayerIDs;

        /// <summary>
        /// Key is requesting ship id
        /// </summary>
        ConcurrentDictionary<int, TradeRequest> _pendingTradeRequests;

        float _tradeRequestTimeoutMS = 10000;
        float _tradeTimeoutMS = 10000;//Easy way to prevent null reference exceptions from concurrency. Trades are kept for a time after processing.
        float _leakTimeoutMS = 50000;//Just in case we for whatever reason fail to process a trade, make sure it still gets removed.

        LocalIDManager _tradeIDManager;

        public EconomyManager(LocalIDManager transactionIDManager, IPlayerLocator pl, IAreaLocator al, CargoSynchronizer cs, ShipManager sm, IDatabaseManager databaseManager, ISlaveIDProvider slaveIDProvider)
        {
            _shipManager = sm;
            _playerLocator = pl;
            _areaLocator = al;
            _cargoSynchronizer = cs;

            _tradeIDManager = transactionIDManager;
            _databaseManager = databaseManager;
            _slaveIDProvider = slaveIDProvider;

            _shipShipTrades = new ConcurrentDictionary<int, ShipShipTrade>();
            _shipIDToTradeID = new ConcurrentDictionary<int, int>();
            _pendingTradeRequests = new ConcurrentDictionary<int,TradeRequest>();
            _tradingPlayerIDs = new HashSet<int>();

        }

        /// <summary>
        /// This should be synchronous
        /// </summary>
        public void Update(object sender, EventArgs e)
        {
            try
            {
                for (int i = 0; i < _shipShipTrades.Count; i++)
                {
                    var t = _shipShipTrades.ElementAt(i);
                    if (t.Value.Accepted_A && t.Value.Accepted_B && t.Value.Pending)
                    {
                        _executeTrade(t.Value);
                    }
                }

                lock (TRADELOCK)
                {

                    for (int i = _shipShipTrades.Count - 1; i >= 0; i--)
                    {
                        var trade = _shipShipTrades.ElementAt(i).Value;

                        if (!trade.Pending && trade.IsProcessed && TimeKeeper.MsSinceInitialization - trade.LastUpdateTime > _tradeTimeoutMS)
                        {
                            ReleaseTrade(trade.Id);
                        }

                    }

                    for (int i = _shipShipTrades.Count - 1; i >= 0; i--)
                    {
                        var trade = _shipShipTrades.ElementAt(i).Value;

                        if (TimeKeeper.MsSinceInitialization - trade.LastUpdateTime > _leakTimeoutMS)
                        {
                            ReleaseTrade(trade.Id);
                        }

                    }

                    for (int i = _pendingTradeRequests.Count-1; i >= 0; i--)
                    {
                        var tradeRequest = _pendingTradeRequests.ElementAt(i).Value;
                        if (TimeKeeper.MsSinceInitialization - tradeRequest.RequestTime > _tradeRequestTimeoutMS)
                        {
                            TradeRequest tr;
                            _pendingTradeRequests.TryRemove(tradeRequest.RequestingShipID, out tr);
                        }

                    }
                }
            }
            catch(Exception ex)
            {
                ConsoleManager.WriteLine(ex);
                ((Timer)sender).Start();
            }

            ((Timer)sender).Start();
        }

        /// <summary>
        /// Establishes a persistent trade request, initializes a pending trade if requestingShip.Id matches a trade previously requested by targetShip
        /// </summary>
        /// <param name="requestingShip"></param>
        /// <param name="targetShip"></param>
        /// <returns></returns>
        public TradeResult ProcessTradeRequest(IShip requestingShip, IShip targetShip)
        {
            lock(TRADELOCK)
            {
                if(_pendingTradeRequests.ContainsKey(targetShip.Id) || _pendingTradeRequests[targetShip.Id].TargetShipID == requestingShip.Id)//If the target of this trade is already requesting a trade with the requesting ship, initiate a trade
                {
                    if(TryInitializeTrade(requestingShip, targetShip) == null)
                    {
                        return TradeResult.ShipAlreadyTrading;
                    }
                    else
                    {
                        //Trade succesfully initialized, remove the pending trade
                        TradeRequest tr;
                        _pendingTradeRequests.TryRemove(targetShip.Id, out tr);
                        return TradeResult.TradeInitialized;
                    }
                }
                
                //If the requestingShip has already requested a trade, just update the request to match the possibly new ID
                _pendingTradeRequests.AddOrUpdate(requestingShip.Id, new TradeRequest(requestingShip.Id, targetShip.Id, TimeKeeper.MsSinceInitialization), (key, existingValue)=>{return existingValue;});
                return TradeResult.WaitingForRequestAccept;
                
            }
        }

        /// <summary>
        /// Returns null if either ship is already trading.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        ShipShipTrade TryInitializeTrade(IShip a, IShip b)
        {
            lock (TRADELOCK)
            {
                if (_shipIDToTradeID.ContainsKey(a.Id))
                    return null;
                if (_shipIDToTradeID.ContainsKey(b.Id))
                    return null;

                Player aPlayer = a.GetPlayer();
                Player bPlayer = b.GetPlayer();

                if (!aPlayer.IsOnline || !bPlayer.IsOnline)
                    return null;

                if(_tradingPlayerIDs.Contains(aPlayer.Id) || _tradingPlayerIDs.Contains(bPlayer.Id))
                    return null;

                ShipShipTrade t = new ShipShipTrade(a, b, _tradeIDManager.PopFreeID(), DateTime.Now, (int)_slaveIDProvider.SlaveID, TimeKeeper.MsSinceInitialization);
                _shipShipTrades.TryAdd(t.Id, t);
                _tradingPlayerIDs.Add(aPlayer.Id);
                _tradingPlayerIDs.Add(bPlayer.Id);
                _shipIDToTradeID.TryAdd(a.Id, t.Id);
                _shipIDToTradeID.TryAdd(b.Id, t.Id);
                return t;
            }
        }
                
        void ReleaseTrade(int tradeId)
        {
            ShipShipTrade tr;
            _shipShipTrades.TryRemove(tradeId, out tr);

            if(tr == null)
            {
                return;
            }

            int tempid;
            _shipIDToTradeID.TryRemove(tr.ShipA.Id, out tempid);
            _shipIDToTradeID.TryRemove(tr.ShipB.Id, out tempid);
            _tradingPlayerIDs.Remove(tr.ShipA.GetPlayer().Id);
            _tradingPlayerIDs.Remove(tr.ShipB.GetPlayer().Id);
            tr.ShipA.GetPlayer().IsTrading = false;
            tr.ShipB.GetPlayer().IsTrading = false;
        }

        /// <summary>
        /// Validates trade request and schedules awaitable transaction sequence
        /// Trades are all or nothing.
        /// Leave typesAndQuantites or statefulCargoIDs null if unused.
        /// </summary>
        /// <param name="sellingPort"></param>
        /// <param name="purchasingShip"></param>
        /// <param name="wareType"></param>
        /// <param name="quantity"></param>
        /// <param name="statefulCargoID"></param>
        /// <returns></returns>
        public async Task<PurchaseResult> TryTradeWithPort(Port port, IShip ship, PortTradeDirection direction, Dictionary<PortWareIdentifier, float> typesAndQuantities, HashSet<int> statefulCargoIDs)
        { 
            switch(direction)
            {
                case PortTradeDirection.ShipSaleToPort:
                    {
                        return await TrySellToPort(ship, port, typesAndQuantities, statefulCargoIDs);
                    }
                case PortTradeDirection.ShipPurchaseFromPort:
                    {
                        return await TryPurchaseCargoFromPort(port, ship, typesAndQuantities, statefulCargoIDs);
                    }
                default:
                    throw new NotImplementedException(direction.ToString() + " not implemented in " + this.GetType().Name);
            }        
        }

        #region Purchasing From Port
        
        async Task<PurchaseResult> TryPurchaseCargoFromPort(Port sellingPort, IShip purchasingShip, Dictionary<PortWareIdentifier, float> typesAndQuantities, HashSet<int> statefulCargoIDs)
        { 

            if (sellingPort.ShipIDs.Contains(purchasingShip.Id))//Make sure the IShip is actually docked
                return PurchaseResult.WrongArea;

            if (typesAndQuantities == null && statefulCargoIDs == null)
                return PurchaseResult.NoItemsSpecified;

            float totalPrice = 0;
            float totalCargoSpace = 0;

            
            List<CargoTransaction> allTransactions = new List<CargoTransaction>();
            PurchaseResult result;

            if(statefulCargoIDs != null)
            {
                result = GeneratePurchaseTransactionSequence(sellingPort, purchasingShip, statefulCargoIDs, ref allTransactions, ref totalPrice, ref totalCargoSpace);
                if(result != PurchaseResult.Success)
                {
                    return result;
                }
            }

            if (typesAndQuantities != null)
            {
                StatelessCargoTypes statelessType;
                var statelessCargoToPurchase = typesAndQuantities.Where(c => { return PortHelper.GetCargoType(c.Key, out statelessType); })
                    .ToDictionary(
                    c => { PortHelper.GetCargoType(c.Key, out statelessType); return statelessType; },
                    c => c.Value);//Map typesAndQuantities to Dictionary<StatelssCargoTypes, float>

                if (statelessCargoToPurchase.Count > 0)
                {
                    result = GeneratePurchaseTransactionSequence(sellingPort, purchasingShip, statelessCargoToPurchase, ref allTransactions, ref totalPrice, ref totalCargoSpace);
                    if (result != PurchaseResult.Success)
                    {
                        return result;
                    }
                }

                StatefulCargoTypes statefulType;
                var statefulCargoToPurchase = typesAndQuantities.Where(c => { return PortHelper.GetCargoType(c.Key, out statefulType); })
                    .ToDictionary(
                    c => { PortHelper.GetCargoType(c.Key, out statefulType); return statefulType; },
                    c => c.Value);//Map typesAndQuantities to Dictionary<StatefulCargoTypes, float>

                if (statefulCargoToPurchase.Count > 0)
                {
                    result = GeneratePurchaseTransactionSequence(sellingPort, purchasingShip, statefulCargoToPurchase, ref allTransactions, ref totalPrice, ref totalCargoSpace);
                    if (result != PurchaseResult.Success)
                    {
                        return result;
                    }
                }
            }
            
            var sequence = new CargoTransactionSequence();
            sequence.Add(new TransactionRemoveStatelessCargo(purchasingShip, StatelessCargoTypes.Cash, totalPrice));
 
            allTransactions.ForEach(ct=>{sequence.Add(ct);});
            _cargoSynchronizer.RequestAtomicTransactionSequence(sequence);

            await sequence.ResultTask;            
            
            if (sequence.ResultTask.Result == CargoResult.Success)
            {
                var record = new PortShipTradeRecordModel(DateTime.Now, (int)_slaveIDProvider.SlaveID)
                {
                    Id = _tradeIDManager.PopFreeID(),
                    Port = sellingPort,
                    Ship = purchasingShip,
                    AllCargoTransactions = sequence.Transactions.Select(tr=>(object)tr).ToList(),                   
                    CashPaid = totalPrice,
                    TransactionDirection = PortTradeDirection.ShipPurchaseFromPort,
                };
                _databaseManager.SaveAsync(record);
                _databaseManager.SaveAsync(purchasingShip);
                _databaseManager.SaveAsync(sellingPort);

                return PurchaseResult.Success;
            }
            else
                return PurchaseResult.FailureReasonUnknown;

        }

        PurchaseResult GeneratePurchaseTransactionSequence(Port sellingPort, IShip purchasingShip, Dictionary<StatefulCargoTypes, float> typesAndQuantities, ref List<CargoTransaction> transactionsToSchedule, ref float totalPrice, ref float totalCargoSpace)
        {

           if (!sellingPort.Cargo.IsCargoForSale(typesAndQuantities))//Make sure the port has the item in stock and for sale
                return PurchaseResult.CargoOutOfStock;
            
            else if (!purchasingShip.Cargo.CheckCargoSpace(typesAndQuantities, ref totalCargoSpace))//Make sure the IShip has enough space
                return PurchaseResult.NotEnoughCargoSpace;
            
            totalPrice += sellingPort.Cargo.GetPrice(typesAndQuantities, PortTradeDirection.ShipPurchaseFromPort);

            if (purchasingShip.Cargo.GetCargoAmount(StatelessCargoTypes.Cash) < totalPrice)
                return PurchaseResult.NotEnoughCash;
            
            else
            {                
                foreach (var t in typesAndQuantities)
                {
                    for (int i = 0; i < (int)t.Value; i++)
                    {
                        transactionsToSchedule.Add(new TransactionRemoveStatefulCargo(sellingPort, t.Key, null, true));
                        transactionsToSchedule.Add(new TransactionAddStatefulCargo(purchasingShip, null, false));                        

                    }
                }

                return PurchaseResult.Success;

            }
        }

        /// <summary>
        /// Used to purchase a specific StatefulCargo object by specifying the ID
        /// </summary>
        /// <param name="sellingPort"></param>
        /// <param name="purchasingShip"></param>
        /// <param name="statefulCargoID"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        PurchaseResult GeneratePurchaseTransactionSequence(Port sellingPort, IShip purchasingShip, HashSet<int> statefulCargoIDs, ref List<CargoTransaction> transactionsToSchedule, ref float totalPrice, ref float totalCargoSpace)
        {
            var cargoForSale = sellingPort.Cargo.IsCargoForSale(statefulCargoIDs.ToList());

            if (cargoForSale.Count != statefulCargoIDs.Count)//Make sure the port has the items in stock and for sale
                return PurchaseResult.CargoOutOfStock;

            else if (!purchasingShip.Cargo.CheckCargoSpace(cargoForSale, ref totalCargoSpace))//Make sure the IShip has enough space
                return PurchaseResult.NotEnoughCargoSpace;

            totalPrice += sellingPort.Cargo.GetPrice(cargoForSale, PortTradeDirection.ShipPurchaseFromPort);

            if (purchasingShip.Cargo.GetCargoAmount(StatelessCargoTypes.Cash) < totalPrice)
                return PurchaseResult.NotEnoughCash;

            else
            {
                foreach(var c in statefulCargoIDs)
                {
                    transactionsToSchedule.Add(new TransactionRemoveStatefulCargo(sellingPort, StatefulCargoTypes.Null, c, true));
                    transactionsToSchedule.Add(new TransactionAddStatefulCargo(purchasingShip, null, false));

                }

                return PurchaseResult.Success;
            }
        }

        PurchaseResult GeneratePurchaseTransactionSequence(Port sellingPort, IShip purchasingShip, Dictionary<StatelessCargoTypes, float> typesAndQuantities, ref List<CargoTransaction> transactionsToSchedule, ref float totalPrice, ref float totalCargoSpace)
        {
 
            foreach(var t in typesAndQuantities)
            {
                if (sellingPort.Cargo.GetCargoAmount(t.Key) < t.Value)//Make sure the port has the items in stock
                    return PurchaseResult.CargoOutOfStock;

            }
            
            if (!purchasingShip.Cargo.CheckCargoSpace(typesAndQuantities, ref totalCargoSpace))//Make sure the IShip has enough space
                return PurchaseResult.NotEnoughCargoSpace;

            totalPrice += sellingPort.Cargo.GetPrice(typesAndQuantities, PortTradeDirection.ShipPurchaseFromPort);

            if (purchasingShip.Cargo.GetCargoAmount(StatelessCargoTypes.Cash) < totalPrice)
                return PurchaseResult.NotEnoughCash;

            else
            {
                foreach (var t in typesAndQuantities)
                {
                    transactionsToSchedule.Add(new TransactionRemoveStatelessCargo(sellingPort, t.Key, t.Value));
                    transactionsToSchedule.Add(new TransactionAddStatelessCargo(purchasingShip, t.Key, t.Value, true));
                }
                return PurchaseResult.Success;
            }

        }
          
        /// <summary>
        /// If ignoreNumAllowed, numUnits can be greater than the max checked in validation. Example: ships can buy more health than MaxHealth
        /// NOTE: if !ignoreNumAllowed, number of units requested may be larger than the number of units purchased.
        /// </summary>
        /// <param name="port"></param>
        /// <param name="serviceRequestData"></param>
        /// <param name="ignoreNumAllowed"></param>
        /// <returns></returns>
        PurchaseResult TryPurchaseService(Port port, PortServiceRequestData serviceRequestData)
        {
            var result = port.ValidatePurchaseResult(serviceRequestData);
            if (result != PurchaseResult.Success)
                return result;



            var t = new TransactionRemoveStatelessCargo(serviceRequestData.ReceivingShip, StatelessCargoTypes.Cash, serviceRequestData.TotalPrice);
            t.OnCompletionData = serviceRequestData;
            t.OnCompletion += port.ExecuteService;
            _cargoSynchronizer.RequestTransaction(t);
            if (t.ResultTask.Result != CargoResult.Success)
                return PurchaseResult.FailureReasonUnknown;//TODO: Implement translator
            else
            {
                serviceRequestData.DateTime = DateTime.Now;
                _databaseManager.SaveAsync(serviceRequestData);
                _databaseManager.SaveAsync(port);
                _databaseManager.SaveAsync(serviceRequestData.ReceivingShip);
                return PurchaseResult.Success;
            }
        }

        #endregion

        #region Selling To Port
       
        public async Task<PurchaseResult> TrySellToPort(IShip sellingShip, Port purchasingPort, Dictionary<PortWareIdentifier, float> typesAndQuantities, HashSet<int> statefulCargoIDs)
        {
            if (purchasingPort.ShipIDs.Contains(sellingShip.Id))//Make sure the IShip is actually docked
                return PurchaseResult.WrongArea;

            if (typesAndQuantities == null && statefulCargoIDs == null)
                return PurchaseResult.NoItemsSpecified;

            float totalPrice = 0;

            List<CargoTransaction> allTransactions = new List<CargoTransaction>();
            PurchaseResult result;

            if (statefulCargoIDs != null)
            {
                result = GenerateSaleTransactionSequence(sellingShip, purchasingPort, statefulCargoIDs, ref allTransactions, ref totalPrice);
                if (result != PurchaseResult.Success)
                {
                    return result;
                }
            }

            if (typesAndQuantities != null)
            {
                StatelessCargoTypes statelessType;
                var statelessCargoToPurchase = typesAndQuantities.Where(c => { return PortHelper.GetCargoType(c.Key, out statelessType); })
                    .ToDictionary(
                    c => { PortHelper.GetCargoType(c.Key, out statelessType); return statelessType; },
                    c => c.Value);//Map typesAndQuantities to Dictionary<StatelssCargoTypes, float>

                if (statelessCargoToPurchase.Count > 0)
                {
                    result = GenerateSaleTransactionSequence(sellingShip, purchasingPort, statelessCargoToPurchase, ref allTransactions, ref totalPrice);
                    if (result != PurchaseResult.Success)
                    {
                        return result;
                    }
                }

                StatefulCargoTypes statefulType;
                var statefulCargoToPurchase = typesAndQuantities.Where(c => { return PortHelper.GetCargoType(c.Key, out statefulType); })
                    .ToDictionary(
                    c => { PortHelper.GetCargoType(c.Key, out statefulType); return statefulType; },
                    c => (int)c.Value);//Map typesAndQuantities to Dictionary<StatefulCargoTypes, float>

                if (statefulCargoToPurchase.Count > 0)
                {
                    result = GenerateSaleTransactionSequence(sellingShip, purchasingPort, statefulCargoToPurchase, ref allTransactions, ref totalPrice);
                    if (result != PurchaseResult.Success)
                    {
                        return result;
                    }
                }

            }

            var sequence = new CargoTransactionSequence();
            
            allTransactions.ForEach(ct => { sequence.Add(ct); });
            sequence.Add(new TransactionAddStatelessCargo(sellingShip, StatelessCargoTypes.Cash, totalPrice, true));
            _cargoSynchronizer.RequestAtomicTransactionSequence(sequence);

            await sequence.ResultTask;

            if (sequence.ResultTask.Result == CargoResult.Success)
            {
                var record = new PortShipTradeRecordModel(DateTime.Now, (int)_slaveIDProvider.SlaveID)
                {
                    Id = _tradeIDManager.PopFreeID(),
                    Port = purchasingPort,
                    Ship = sellingShip,
                    AllCargoTransactions = sequence.Transactions.Select(tr => (object)tr).ToList(),
                    CashPaid = totalPrice,
                    TransactionDirection = PortTradeDirection.ShipSaleToPort,
                };
                _databaseManager.SaveAsync(record);
                _databaseManager.SaveAsync(sellingShip);
                _databaseManager.SaveAsync(purchasingPort);

                return PurchaseResult.Success;
            }
            else
                return PurchaseResult.FailureReasonUnknown;
        }

        PurchaseResult GenerateSaleTransactionSequence(IShip sellingShip, Port purchasingPort, HashSet<int> statefulCargoIDs, ref List<CargoTransaction> transactionsToSchedule, ref float totalPrice)
        {            
            var cargoToSell = sellingShip.Cargo.GetStatefulCargo(statefulCargoIDs.ToList());
            if(cargoToSell.Count != statefulCargoIDs.Count)
            {
                return PurchaseResult.CargoNotInHolds;
            }

            totalPrice += purchasingPort.Cargo.GetPrice(cargoToSell, PortTradeDirection.ShipSaleToPort);
                    
                
            foreach(var i in statefulCargoIDs)
            {
                var fromShip = new TransactionRemoveStatefulCargo(sellingShip, StatefulCargoTypes.Null, i, true);
                var toPort = new TransactionAddStatefulCargo(purchasingPort, null, true);

                transactionsToSchedule.Add(fromShip);
                transactionsToSchedule.Add(toPort);
            }

            return PurchaseResult.Success;

                
            
        }

        PurchaseResult GenerateSaleTransactionSequence(IShip sellingShip, Port purchasingPort, Dictionary<StatefulCargoTypes, int> typeAndQuantity, ref List<CargoTransaction> transactionsToSchedule, ref float totalPrice)
        { 
            foreach(var v in typeAndQuantity)
            {
                if (!sellingShip.Cargo.IsCargoInHolds(v.Key, v.Value))
                    return PurchaseResult.CargoOutOfStock;
                else
                {
                    totalPrice += purchasingPort.Cargo.GetPrice(v.Key, v.Value, PortTradeDirection.ShipSaleToPort);
                }
            }

               
            foreach(var v in typeAndQuantity)
            {
                for(int i = 0; i < v.Value; i++)
                {
                    transactionsToSchedule.Add(new TransactionRemoveStatefulCargo(sellingShip, v.Key, null, true));
                    transactionsToSchedule.Add(new TransactionAddStatefulCargo(purchasingPort, null, true));
                }
            }

            return PurchaseResult.Success;
        }

        PurchaseResult GenerateSaleTransactionSequence(IShip sellingShip, Port purchasingPort, Dictionary<StatelessCargoTypes, float> typeAndQuantity, ref List<CargoTransaction> transactionsToSchedule, ref float totalPrice)
        {
            foreach (var v in typeAndQuantity)
            {
                if (!sellingShip.Cargo.IsCargoInHolds(v.Key, v.Value))
                    return PurchaseResult.CargoOutOfStock;
                else
                {
                    totalPrice += purchasingPort.Cargo.GetPrice(v.Key, v.Value, PortTradeDirection.ShipSaleToPort);
                }
            }

            foreach (var v in typeAndQuantity)
            {
                for (int i = 0; i < v.Value; i++)
                {
                    transactionsToSchedule.Add(new TransactionRemoveStatelessCargo(sellingShip, v.Key, v.Value));
                    transactionsToSchedule.Add(new TransactionAddStatelessCargo(purchasingPort, v.Key, v.Value, true));                    
                }
            }

            return PurchaseResult.Success;   
        }


        #endregion

        #region Ship-Ship trading

        public TradeResult SetTradeAcceptStatus(int tradingShipID, bool accept)
        {
            int tradeId;
            

            if(!_shipIDToTradeID.TryGetValue(tradingShipID, out tradeId))
            {
                return TradeResult.TradingEntityNotFound;
            }

            ShipShipTrade trade;

            if(!_shipShipTrades.TryGetValue(tradeId, out trade))
            {
                return TradeResult.TradeNotFound;
            }

            if(trade.ShipA.Id == tradingShipID)
            {
                trade.Accepted_A = accept;
                return TradeResult.Success;
            }
            else if(trade.ShipB.Id == tradingShipID)
            {
                trade.Accepted_B = accept;
                return TradeResult.Success;
            }
            else
            {
                throw new CorruptStateException("tradingShipID did not correspond to either of the IDs in a ShipShipTrade.");//This would be a strange exception to encounter.
            }
                                     
            
        }
        
        /// <summary>
        /// Returns the most recent ShipShipTrade if update is succesful, null otherwise.
        /// Designed such that clients send trade data only when they attempt to update offered cargo. The result is pushed to both clients containing the latest update if it is succesful. This way, no polling necessary.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ShipShipTrade UpdateTradeData(TradeData data)
        {
            lock (TRADELOCK)
            {
                ShipShipTrade trade;
                int tradeId;
                if (!_shipIDToTradeID.TryGetValue(data.ShipID, out tradeId))
                {
                    return null;
                }
                if (!_shipShipTrades.TryGetValue(tradeId, out trade))
                {
                    return null;
                }

                CargoHandlerModel tradeCargo;
                CargoHandler_ReadOnlyVM<CargoHandlerModel> shipCargo;

                if (data.ShipID == trade.ShipA.Id)
                {                    
                    tradeCargo = trade.CargoOffered_A;
                    shipCargo = trade.ShipA.GetCargo();
                                       
                }
                else if (data.ShipID == trade.ShipB.Id)
                {
                    tradeCargo = trade.CargoOffered_B;
                    shipCargo = trade.ShipB.GetCargo();
                }
                else
                {
                    throw new CorruptStateException("TradeData id was not consistent with IDs stored in the retrieved ShipShipTrade object.");
                }

                trade.LastUpdateTime = TimeKeeper.MsSinceInitialization;

                var newStatefulCargo = new Dictionary<int, StatefulCargo>();
                foreach(var c in data.CargoData.StatefulCargo)
                {
                    //Check to make sure the ship holdss the cargo
                    var cargoType = shipCargo.IsCargoInHolds(c.Id);

                    if(cargoType != StatefulCargoTypes.Null)
                    {
                        if(cargoType != c.CargoType)
                        {
                            //Unlikely, but might as well check
                            throw new CorruptStateException("CargoType of cargo object sent with TradeData update did not match CargoType of StatefulCargo with the corresponding cargo ID.");
                        }

                        tradeCargo.StatefulCargo.Add(c.Id, shipCargo.GetStatefulCargo(c.Id));
                    }
                    else
                    {
                        return null;//Cargo wasn't in holds, trade update fails.
                    }                    
                }
                tradeCargo.StatefulCargo = newStatefulCargo;

                var newStatelessCargo = new Dictionary<StatelessCargoTypes, StatelessCargo>();
                foreach (var c in data.CargoData.StatelessCargo)
                {
                    if (shipCargo.IsCargoInHolds(c.CargoType, c.Quantity))
                    {
                        newStatelessCargo.Add(c.CargoType, new StatelessCargo(c.CargoType, c.Quantity));
                    }
                    else
                    {
                        return null;//Cargo wasn't in holds, trade update fails.
                    }
                }
                tradeCargo.StatelessCargo = newStatelessCargo;

                return trade;
            }

            
            

        }

        async Task<TradeResult> _executeTrade(ShipShipTrade t)
        {
            if (t.IsProcessed || !t.Pending)
            {
                return TradeResult.TradeAlreadyProcessed;
    
            }
            t.Pending = false;
            CargoHandler_ReadOnlyVM<CargoHandlerModel> cargoA = t.ShipA.GetCargo();
            CargoHandler_ReadOnlyVM<CargoHandlerModel> cargoB = t.ShipB.GetCargo();


            float holdsAfterTrade_A = cargoA.FilledHolds - t.CargoOffered_A.FilledHolds + t.CargoOffered_B.FilledHolds;
            float holdsAfterTrade_B = cargoB.FilledHolds - t.CargoOffered_B.FilledHolds + t.CargoOffered_A.FilledHolds;

            if(holdsAfterTrade_A > cargoA.TotalHolds)
            {
                t.IsProcessed = true;
                return TradeResult.ShipANotEnoughCargoSpace;
                
            }

            if(holdsAfterTrade_B > cargoB.TotalHolds)
            {
                t.IsProcessed = true;
                return TradeResult.ShipBNotEnoughCargoSpace;
            }

            
            CargoTransactionSequence ts = new CargoTransactionSequence();
            foreach (var v in t.CargoOffered_A.StatefulCargo)
            {
                ts.Add(new TransactionRemoveStatefulCargo(t.ShipA, StatefulCargoTypes.Null, v.Value.Id, true));
                ts.Add(new TransactionAddStatefulCargo(t.ShipB, null, true));                
            }

            foreach (var v in t.CargoOffered_B.StatefulCargo)
            {
                ts.Add(new TransactionRemoveStatefulCargo(t.ShipB, StatefulCargoTypes.Null, v.Value.Id, true));
                ts.Add(new TransactionAddStatefulCargo(t.ShipA, null, true));
            }

            foreach (var v in t.CargoOffered_A.StatelessCargo)
            {
                ts.Add(new TransactionRemoveStatelessCargo(t.ShipA, v.Key, v.Value.Quantity));
                ts.Add(new TransactionAddStatelessCargo(t.ShipB, v.Key, v.Value.Quantity, true));
            }

            foreach (var v in t.CargoOffered_B.StatelessCargo)
            {
                ts.Add(new TransactionRemoveStatelessCargo(t.ShipB, v.Key, v.Value.Quantity));
                ts.Add(new TransactionAddStatelessCargo(t.ShipA, v.Key, v.Value.Quantity, true));
            }

            _cargoSynchronizer.RequestAtomicTransactionSequence(ts);

            var res = await ts.ResultTask;

            
            if (res == CargoResult.Success)
            {
                t.IsProcessed = true;

                //Record the trade in the DB, fire and forget
                t.DateTime = DateTime.Now;
                _databaseManager.SaveAsync(t);
                _databaseManager.SaveAsync((ISerializable)t.ShipA);
                _databaseManager.SaveAsync((ISerializable)t.ShipB);

                return TradeResult.Success;
            }
            else
                return TradeResult.FailedReasonUnknown;//TODO: Implement translator

        }

        /// <summary>
        /// Cancels a trade (if it isn't currently being executed). Returns the trade if it is found and cancelled.
        /// </summary>
        /// <param name="tradingShipID"></param>
        public ShipShipTrade TerminateTrade(int tradingShipID, bool sendNotification)
        {
            lock (TRADELOCK)
            {
                int tradeId;
                if (!_shipIDToTradeID.TryGetValue(tradingShipID, out tradeId))
                {
                    return null;
                }
                var trade = _shipShipTrades[tradeId];
                if (!trade.IsProcessed && trade.Pending)
                {
                    trade.IsProcessed = true;
                    trade.Pending = false;

                    if(sendNotification)
                    {
                        trade.ShipA.GetPlayer().SendMessage(new NetworkMessageContainer(new MessageEmptyMessage(), MessageTypes.CancelShipTrade));
                        trade.ShipB.GetPlayer().SendMessage(new NetworkMessageContainer(new MessageEmptyMessage(), MessageTypes.CancelShipTrade));
                    }

                    return trade;
                }
                else
                {
                    return null;
                }
            }
        }

        class TradeRequest
        {
            public int RequestingShipID;
            public int TargetShipID;
            public float RequestTime;

            public TradeRequest(int requestingShipID, int targetShipID, float requestTime)
            {
                RequestingShipID = requestingShipID;
                TargetShipID = targetShipID;
                RequestTime = requestTime;
            }

        }


        #endregion

    }

    public interface ITradeTerminator
    {
        /// <summary>
        /// Cancels a trade (if it isn't currently being executed). Returns the trade if it is found and cancelled.
        /// </summary>
        /// <param name="tradingShipID"></param>
        ShipShipTrade TerminateTrade(int tradingShipID, bool sendNotification);
    }
    
    


}