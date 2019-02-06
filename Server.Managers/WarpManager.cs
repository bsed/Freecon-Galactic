using System;
using SRServer.Services;
using Server.Models;
using Freecon.Models.TypeEnums;
using Server.Interfaces;
using Freecon.Core.Networking.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core;
using Freecon.Core.Interfaces;
using Freecon.Core.Networking.ServerToServer;
using RedisWrapper;
using Server.Database;

namespace Server.Managers
{
    public class WarpManager:IWarpManager
    {
        IAreaLocator _areaLocator;
        MessageManager _messageManager;
        ChatManager _chatManager;
        private RedisServer _redisServer;
        private AccountManager _accountManager;
        private GalaxyRegistrationManager _registrationManager;
        private IDatabaseManager _databaseManager;

        public WarpManager(IAreaLocator al, MessageManager mm, ChatManager chatManager, RedisServer redisServer, AccountManager accountManager, IDatabaseManager databaseManager)
        {
            _areaLocator = al;
            _messageManager = mm;
            _chatManager = chatManager;
            _redisServer = redisServer;
            _accountManager = accountManager;
            _databaseManager = databaseManager;
        }

        /// <summary>
        /// This is gross, I know, just go with it.
        /// </summary>
        /// <param name="rm"></param>
        public void SetRegistrationManager(GalaxyRegistrationManager rm)
        {
            _registrationManager = rm;
        }
        
        #region Area Changing

        /// <summary>
        /// Changes the ship's area appropriately.
        /// Handles associated player if player.ActiveShipId == ship.Id
        /// Initiates a handoff, if the destination is a system and is non-local
        /// </summary>
        /// <param name="newAreaID"></param>
        /// <param name="ship"></param>
        /// <param name="isWarping"></param>
        /// <param name="databaseManager">Set to null if area is local</param>
        /// <param name="isDestinationSystem">True if newAreaID corresponds to a system. False otherwise.</param>
        public async Task ChangeArea(int newAreaID, IShip ship, bool isWarping, bool writeToDb = true)
        {
            Player p = ship.GetPlayer();
            if (!_areaLocator.IsLocalArea(newAreaID))
            {
                var nextArea = await _databaseManager.GetAreaAsync(newAreaID);
                if (nextArea == null)
                {
                    throw new InvalidOperationException("AreaID not found in database!");
                }                

                await HandoffInitiateAreaChange(newAreaID, nextArea.AreaType, ship, ship.GetPlayer());
                return;
            }
            else
            {
                bool isActiveShip = p.ActiveShipId == ship.Id;
                IArea currentArea = _areaLocator.GetArea(ship.CurrentAreaId);
                IArea newArea = _areaLocator.GetArea(newAreaID);

                MoveShipLocal(ship, newAreaID);
                if (isActiveShip)
                    MovePlayerLocal(ship.GetPlayer(), newAreaID, isWarping);

                if (writeToDb)
                {
                    var writeTasks = new List<Task>()
                    {
                        _databaseManager.SaveAsync(ship),
                        _databaseManager.SaveAsync(_areaLocator.GetArea(newAreaID))
                    };
                    if (isActiveShip)
                        writeTasks.Add(_databaseManager.SaveAsync(p));

                    if (currentArea != null) writeTasks.Add(_databaseManager.SaveAsync(currentArea));

                    await Task.WhenAll(writeTasks);
                }

                ValidateState(ship, p, newArea, currentArea);
            }
        }
        
        /// <summary>
        /// Changes the ship's area appropriately.
        /// Handles associated player, if needed
        /// </summary>
        /// <param name="newAreaID"></param>
        /// <param name="ship"></param>
        /// <param name="databaseManager"></param>
        public void ChangeArea(int? newAreaID, NPCShip ship)
        {
            ConsoleManager.WriteLine("ERROR: ChangeArea not implemented for NPCs!", ConsoleMessageType.Error);
            return;
            
        }
        
        public void MovePlayerLocal(Player p, int destinationAreaId, bool isWarping)
        {
            if (!_areaLocator.IsLocalArea(destinationAreaId))
                return;

            IArea currentArea = null;

            currentArea = _areaLocator.GetArea((int)p.CurrentAreaID);

            var newArea = _areaLocator.GetArea(destinationAreaId);
                        
            AreaTypes currentAreaType = currentArea?.AreaType ?? AreaTypes.Any;     

            if (currentAreaType == AreaTypes.System)
                newArea.MovePlayerHere(p, isWarping);
            else
                newArea.MovePlayerHere(p, false);

            p.SetArea(newArea);
            
            if (p.PlayerType == PlayerTypes.Human && p.IsOnline)
            {
                newArea.SendEntryData((HumanPlayer)p, isWarping, p.GetActiveShip());
            }            
        }

        public void MoveShipLocal(IShip ship, int destinationAreaId)
        {
            if (!_areaLocator.IsLocalArea(destinationAreaId))
                return;

            IArea currentArea = null;
            if (ship.CurrentAreaId != null)
            {
                currentArea = _areaLocator.GetArea((int)ship.CurrentAreaId);
            }

            var newArea = _areaLocator.GetArea(destinationAreaId);
            
            AreaTypes currentAreaType = currentArea?.AreaType ?? AreaTypes.Any;

            OnAreaExit(ship, currentArea, newArea.AreaType);
            newArea.MoveShipHere(ship);
            ship.SetArea(newArea);
                        
            newArea.SetEntryPosition(ship, currentArea);
                
            OnAreaChange(ship, newArea, currentAreaType);
        }

        /// <summary>
        /// Call this to begin changing area when handing off a local client.
        /// </summary>
        /// <param name="newArea"></param>
        /// <param name="ship"></param>
        /// <param name="isWarping"></param>
        async Task HandoffInitiateAreaChange(int destinationAreaID, AreaTypes newAreaType, IShip ship, Player warpingPlayer)
        {
            var shipSaveTask = _databaseManager.SaveAsync(warpingPlayer);
            var playerSaveTask = _databaseManager.SaveAsync(ship);
            var shipSaveHandoffTask = _databaseManager.HandoffSaveAsync(ship);//If these collections aren't empty on server startup, a server crashed mid handoff and there could be ships/players which aren't stored in any area and might not be loaded.
            var playerSaveHandoffTask = _databaseManager.HandoffSaveAsync(warpingPlayer);

            //Ensure that db versions of ship and player are most recent, since they'll be loaded by slave
            List<Task> tasklist = new List<Task>
            {
                shipSaveTask,
                shipSaveHandoffTask,
                playerSaveTask,
                playerSaveHandoffTask,               
            };

            await Task.WhenAll(tasklist);

            if (!(shipSaveTask.Result.IsAcknowledged && shipSaveHandoffTask.Result.IsAcknowledged && playerSaveTask.Result.IsAcknowledged && playerSaveHandoffTask.Result.IsAcknowledged))
            {
                throw new InvalidOperationException("Error: db write failed during handoff, handoff aborted.");
            }

            //Send handoff to be handled by server                                
            NetworkMessageContainer redismsg = new NetworkMessageContainer(new MessageClientHandoff((int)warpingPlayer.AccountID, (int)warpingPlayer.ActiveShipId, destinationAreaID, ((LidgrenOutgoingMessageService)warpingPlayer.MessageService).Connection.RemoteEndPoint.Address.GetAddressBytes()), MessageTypes.Redis_ClientHandoff);
            _redisServer.PublishObject(MessageTypes.Redis_ClientHandoff, redismsg);

            IArea currentArea = null;
            if (ship.CurrentAreaId != null)
                currentArea = _areaLocator.GetArea((int)ship.CurrentAreaId);

            OnAreaExit(ship, currentArea, newAreaType);

            currentArea.RemovePlayer(ship.GetPlayer());//There doesn't seem to be a convenient way to avoid putting this here
            currentArea.RemoveShip(ship);//Sends removeship Command to clients in area
            ConsoleManager.WriteLine("Initiated Handoff");
            ship.GetPlayer().IsHandedOff = true;

            _registrationManager.DeRegisterObject(warpingPlayer);
            _registrationManager.DeRegisterObject(ship);
            _accountManager.DeregisterAccount(warpingPlayer.GetAccount());

        }

        /// <summary>
        /// Call this to complete changing area when recieving client on handoff
        /// </summary>
        /// <param name="newArea"></param>
        /// <param name="ship"></param>
        /// <param name="isWarping"></param>
        public async Task HandoffCompleteAreaChange(int newAreaID, IShip ship, Player player)
        {
            //There's a small chance that setting areas to null won't work properly...we'll find out soon!
            ship.SetArea(null);
            player.SetArea(null);
            await ChangeArea(newAreaID, ship, true);

            //If these collections aren't empty on server startup, a server crashed mid handoff and there could be ships/players which aren't stored in any area and might not be loaded.
            _databaseManager.HandoffDeleteAsync(ship.Id, ModelTypes.ShipModel);
            _databaseManager.HandoffDeleteAsync(player.Id, ModelTypes.PlayerModel);

            ConsoleManager.WriteLine("Completed Handoff.");

        }

        /// <summary>
        /// To be called just after a IShip warps
        /// </summary>
        /// <param name="s"></param>
        /// <param name="currentArea">The area that the IShip warped into</param>
        /// <param name="oldArea">The area that the IShip left</param>
        void OnAreaChange(IShip s, IArea currentArea, AreaTypes oldArea)
        {

            if (currentArea == null)
                return;

            var player = s.GetPlayer();
            if (player?.ActiveShipId ==s.Id)
                switch (currentArea.AreaType)
                {
                    case AreaTypes.StarBase:
                    case AreaTypes.Port:
                        switch (oldArea)
                        {
                            case AreaTypes.StarBase:
                            case AreaTypes.Port: // How the heck will you ever get here? I dunno bro, just go with it.
                                _chatManager.BroadcastChatAlert(currentArea, player.Id, player.Username + " has gone through a wormhole.");
                                break;
                            case AreaTypes.System:
                                _chatManager.BroadcastChatAlert(currentArea, player.Id, player.Username + " has entered the port.");
                                break;
                        }
                        break;
                    case AreaTypes.System:
                        switch (oldArea)
                        {
                            case AreaTypes.StarBase:
                            case AreaTypes.Port:
                                _chatManager.BroadcastChatAlert(currentArea, player.Id, player.Username + " has undocked from the port.");
                                break;
                            case AreaTypes.System:
                                _chatManager.BroadcastChatAlert(currentArea, player.Id, player.Username + " has warped into the system.");
                                break;
                        }
                        break;
                }

        }
        
        /// <summary>
        /// To be called just before a IShip warps
        /// </summary>
        /// <param name="s"></param>
        /// <param name="currentArea">The area the IShip is about to leave</param>
        /// <param name="newArea">The area the IShip is about to warp to</param>
        void OnAreaExit(IShip s, IArea currentArea, AreaTypes newArea)
        {
            if (currentArea == null)
            {
                return;
            }            

            var player = s.GetPlayer();
            if (player.ActiveShipId == s.Id)
                switch (currentArea.AreaType) 
                {
                    case AreaTypes.StarBase:
                    case AreaTypes.Port:
                        switch (newArea)
                        {
                            case AreaTypes.StarBase:
                            case AreaTypes.Port:
                                _chatManager.BroadcastChatAlert(currentArea, player.Id, player.Username + " has entered a wormhole.");
                                break;
                            case AreaTypes.System:
                                _chatManager.BroadcastChatAlert(currentArea, player.Id, player.Username + " has undocked.");
                                break;
                        }
                        break;
                    case AreaTypes.System:
                        switch (newArea)
                        {
                            case AreaTypes.StarBase:
                            case AreaTypes.Port:
                                _chatManager.BroadcastChatAlert(currentArea, player.Id, player.Username + " has docked at the port.");
                                break;
                            case AreaTypes.System:
                                _chatManager.BroadcastChatAlert(currentArea, player.Id, player.Username + " has warped out of the system.");
                                break;
                        }
                        break;
            }
            

        }
        
        /// <summary>
        /// Just validation for now; if we start seeing these errors we can take steps to prevent/correct them
        /// NOT FOR HANDOFFS, WILL FAIL INCORRECTLY FOR HANDOFFS
        /// </summary>
        /// <param name="warpingShip"></param>
        /// <param name="warpingPlayer"></param>
        /// <param name="destinationArea"></param>
        void ValidateState(IShip warpingShip, Player warpingPlayer, IArea destinationArea, IArea previousArea)
        {
            if(destinationArea == null && warpingShip.CurrentAreaId != null)
            {
                throw new CorruptStateException("After warp attempt, destinationArea and warpingShip.CurrentAreaID are not both null");
            }

            if(destinationArea == null && warpingPlayer.CurrentAreaID != null)
            {
                throw new CorruptStateException("After warp attempt, destinationArea and warpingPlayer.CurrentAreaID are not both null");
            }


            if(warpingShip.CurrentAreaId != destinationArea.Id)
            {
                throw new CorruptStateException("After warp attempt, warpingShip.CurrentAreaID != destinationArea.Id");
            }

            if(warpingPlayer.CurrentAreaID != destinationArea.Id)
            {
                throw new CorruptStateException("After warp attempt, warpingPlayer.CurrentAreaID != destinationArea.Id");
            }

            if(!destinationArea.ShipIDs.Contains(warpingShip.Id))
            {
                throw new CorruptStateException("After warp attempt, warpingShip is not contained in the destination area");
            }

            if(warpingPlayer.IsOnline && !destinationArea.OnlinePlayerIDs.Contains(warpingPlayer.Id))
            {
                throw new CorruptStateException("After warp attempt, warpingPlayer is not contained in the destination area");
            }




            if (previousArea == null)
            {
                return;
            }

            if (destinationArea != previousArea && previousArea.ShipIDs.Contains(warpingShip.Id))
            {
                throw new CorruptStateException("After warp attempt, warpingShip is still contained in the previous area");
            }

            if (destinationArea != previousArea && previousArea.OnlinePlayerIDs.Contains(warpingPlayer.Id))
            {
                throw new CorruptStateException("After warp attempt, warpingPlayer is still contained in the previous area");
            }

        }


        #endregion


    }
}
