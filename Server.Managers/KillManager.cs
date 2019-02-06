using System;
using System.Collections.Generic;
using Server.Models.Interfaces;
using Server.Models;
using Freecon.Core.Networking.Models;
using Server.Models.Structures;
using Server.Models.Mathematics;
using SRServer.Services;
using Freecon.Core.Interfaces;
using Server.Interfaces;
using Server.Managers.Synchronizers.Transactions;
using Freecon.Models.TypeEnums;
using Freecon.Core.Networking.Models.Messages;
using Freecon.Core.Utils;
using Freecon.Models;
using Server.Managers.Economy;

namespace Server.Managers
{
    /// <summary>
    /// Handles killing of all killable objects in the game
    /// </summary>
    public class KillManager
    {
        MessageManager _messageManager;
        ConnectionManager _connectionManager;
        WarpManager _warpManager;
        IAreaLocator _areaLocator;
        IPlayerLocator _playerLocator;
        CargoSynchronizer _cargoSynchronizer;
        ChatManager _chatManager;
        ITradeTerminator _tradeTerminator; 

        Random rand = new Random(65464);

        public KillManager(CargoSynchronizer cargoSynchronizer, IPlayerLocator pl, IAreaLocator al, MessageManager mm, ConnectionManager cm, WarpManager wm, ChatManager chatManager, ITradeTerminator tradeTerminator)
        {
            _cargoSynchronizer = cargoSynchronizer;
            _messageManager = mm;
            _connectionManager = cm;
            _warpManager = wm;
            _areaLocator = al;
            _playerLocator = pl;
            _chatManager = chatManager;
            _tradeTerminator = tradeTerminator;
        }

        public IKillable Kill(IKillable obj, ICanFire killingObject)
        {
            if (obj is IShip)
                KillShip((IShip)obj, killingObject);
            else if (obj is Turret)
                KillTurret((Turret)obj);
            else
                throw new Exception("Kill not implemented in KillManager for object of type " + obj.GetType());


            return obj;
        }

        public IShip KillShip(IShip s, ICanFire killingObject)
        {
            if(s.IsDead)
            {
                ConsoleManager.WriteLine("Killing a ship which was already dead.", ConsoleMessageType.Warning);
                return s;
            }


            s.IsDead = true;
            s.KillTimeStamp = TimeKeeper.MsSinceInitialization;
            s.RespawnTimeDelay = 3000;//TODO: This will be a problem later, if a IShip warps into a new system where a dead IShip is waiting to respawn, the warping IShip will see a live ship. Needs to be fully implemented.
            s.CurrentHealth = 0;

            if(s.GetPlayer().IsTrading)
            {
                _tradeTerminator.TerminateTrade(s.Id, true);
            }

            var currentArea = s.GetArea();

            if (currentArea.NumOnlinePlayers > 0)
            {

                MessageRemoveKillRevive msgData = new MessageRemoveKillRevive();
                msgData.ActionType = ActionType.Kill;
                msgData.ObjectType = RemovableObjectTypes.Ship;
                msgData.ObjectIDs.Add(s.Id);
                
                currentArea.BroadcastMessage(new NetworkMessageContainer(msgData, MessageTypes.RemoveKillRevive));

                // Send chat messages
                if (killingObject is IShip)
                {
                    ((IShip) killingObject).GetPlayer().PlayersKilled++;
                    var killingPlayer = ((IShip)killingObject).GetPlayer();

                    killingPlayer.PlayersKilled++;

                    var killText = string.Format("{0} was shot down by {1}!", s.GetPlayer().Username, killingPlayer.Username);

                    _chatManager.BroadcastSimpleChat(s.GetArea(), "", killText, ChatTypes.None);
                }
                else if (killingObject is Turret)
                {
                    _playerLocator.GetPlayerAsync(((Turret)killingObject).OwnerID).Result.PlayersKilled++;

                    var killedPlayer = s.GetPlayer();

                    var defensesOwner = _playerLocator.GetPlayerAsync(((Turret) killingObject).OwnerID).Result.Username;

                    var killText = string.Format("{0} was shot down by defenses of {1}!", killedPlayer.Username, defensesOwner);
                    
                    _chatManager.BroadcastSimpleChat(s.GetArea(), "", killText, ChatTypes.None);

                }
            }

            #region Modules
            //For now, just make the ship drop one mod to space. Later we'll figure out how many to destroy/keep with the ship
            var moduleToRemove = s.Cargo.GetAnyStatefulCargo(StatefulCargoTypes.Module);
            if (moduleToRemove != null)
            {
                var ct = new TransactionRemoveStatefulCargo(s, StatefulCargoTypes.Module, moduleToRemove.Id);
                ct.OnCompletion += s.CargoRemoved;
                ct.OnCompletion += _messageManager.NotifyCargoRemoved;
                ct.OnCompletion += _addCargoToArea;
                

                var mod = moduleToRemove as Module;
                mod.PosX = s.PosX;
                mod.PosY = s.PosY;
                mod.Rotation = s.Rotation;
                mod.NextAreaID = (int)s.CurrentAreaId;
               
                _cargoSynchronizer.RequestTransaction(ct);

            }


            #endregion


            s.CurrentHealth = s.ShipStats.MaxHealth;
            //s.IsDead = false;
            float tempx = 0;
            float tempy = 0;
            SpatialOperations.GetRandomPointInRadius(ref tempx, ref tempy, 10, 20);

            s.PosX = tempx;
            s.PosY = tempy;
                       

            return s;
        }

        public void KillTurret(Turret t)
        {
            if (t.IsDead)
                return;

            t.IsDead = true;
            
            var area = _areaLocator.GetArea(t.CurrentAreaId);
            if (area.NumOnlinePlayers > 0)
            {

                MessageRemoveKillRevive msgData = new MessageRemoveKillRevive();
                msgData.ActionType = ActionType.Kill;
                msgData.ObjectType = RemovableObjectTypes.Structure;
                msgData.ObjectIDs.Add(t.Id);
                area.BroadcastMessage(new NetworkMessageContainer(msgData, MessageTypes.RemoveKillRevive));
            }


        }


        /// <summary>
        /// TODO: clean this nasty mess up.
        /// </summary>
        void _addCargoToArea(object sender, ITransactionRemoveStatefulCargo tr)
        {
            IFloatyAreaObject fa = (IFloatyAreaObject)tr.RemovedCargo;
            
            List<IFloatyAreaObject> l = new List<IFloatyAreaObject> { fa };
            IArea a = _areaLocator.GetArea(fa.NextAreaID);
            a.AddFloatyAreaObjects(l);
            ConsoleManager.WriteLine("NPC dropped " + tr.RemovedCargo.GetType().Name, ConsoleMessageType.Notification);
               
                
            

        }
                
    }
}