using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Logging;
using Core.Models.CargoHandlers;
using Core.Models.Enums;
using Core.Utilities;
using Freecon.Core.Interfaces;
using Freecon.Core.Networking.Models;
using Freecon.Core.Networking.Models.Messages;
using Freecon.Core.Networking.Models.Objects;
using Freecon.Core.Utils;
using Freecon.Models;
using Freecon.Models.ChatCommands;
using Freecon.Models.TypeEnums;
using Lidgren.Network;
using Server.Factories;
using Server.Interfaces;
using Server.Managers;
using Server.Managers.ChatCommands;
using Server.Managers.Synchronizers.Transactions;
using Server.Models;
using Server.Models.Mathematics;
using Server.Models.Structures;
using Core.Models;
using Freecon.Core;
using Freecon.Core.Utilities;
using Freecon.Models.UI;
using Freecon.Core.Networking.Messages;
using Freecon.Core.Networking.Objects;

namespace SRServer
{
    partial class Server
    {
        public ServerNetworkMessage PreprocessLidgrenMessage(NetIncomingMessage receivedMessage)
        {
            switch (receivedMessage.MessageType)
            {
                case NetIncomingMessageType.VerboseDebugMessage:
                    ConsoleManager.WriteLine(receivedMessage.ReadString(), ConsoleMessageType.NetworkMessage);
                    return null;
                case NetIncomingMessageType.DebugMessage:
                    ConsoleManager.WriteLine(receivedMessage.ReadString(), ConsoleMessageType.NetworkMessage);
                    return null;
                case NetIncomingMessageType.WarningMessage:
                    ConsoleManager.WriteLine(receivedMessage.ReadString(), ConsoleMessageType.NetworkMessage);
                    return null;
                case NetIncomingMessageType.ErrorMessage:
                    ConsoleManager.WriteLine(receivedMessage.ReadString(), ConsoleMessageType.NetworkMessage);
                    return null;
                case NetIncomingMessageType.ConnectionApproval: //To be replaced with SSL, node.js, and NGINX
                {
                    HandleConnectionApproval(receivedMessage);
                    return null;
                }
                case NetIncomingMessageType.ConnectionLatencyUpdated:
                {
                    ConsoleManager.WriteLine(receivedMessage.ReadFloat().ToString());
                    break;
                }
                case NetIncomingMessageType.StatusChanged:
                {
                    HandleStatusChanged(receivedMessage);
                    break;
                }

                case NetIncomingMessageType.Data:
                {
                    var deserialized = SerializationUtilities.DeserializeMsgPack<NetworkMessageContainer>(receivedMessage);

                    var hasPlayerConnection = _playerManager.connectionToPlayer.ContainsKey(receivedMessage.SenderConnection);

                    var player = hasPlayerConnection ? _playerManager.connectionToPlayer[receivedMessage.SenderConnection] : null;

                    return new ServerNetworkMessage(deserialized, player);
                }

                default:
                {
                    if (receivedMessage.MessageType == NetIncomingMessageType.Error)
                    {
                        ConsoleManager.WriteLine(
                            "Lidgren error message received: " + receivedMessage.MessageType + ": " +
                            receivedMessage.ReadString(), ConsoleMessageType.NetworkMessage
                        );
                    }
                    else
                    {

                        ConsoleManager.WriteLine(
                            receivedMessage.MessageType + ": " + receivedMessage.ReadString(),
                            ConsoleMessageType.NetworkMessage
                        );
                    }

                    return null;
                }
            }

            return null;
        }

        private void HandleStatusChanged(NetIncomingMessage receivedMessage)
        {
            if (receivedMessage.SenderConnection.Status == NetConnectionStatus.Disconnecting
                || receivedMessage.SenderConnection.Status == NetConnectionStatus.Disconnected)
            {
                if (_playerManager.connectionToPlayer.ContainsKey(receivedMessage.SenderConnection))
                {
                    Player p = _playerManager.connectionToPlayer[receivedMessage.SenderConnection];

                    if (!p.IsHandedOff) //Player hasn't been handed off, this is a disconnection
                    {
                        ConsoleManager.WriteLine("Player " + p.Username + " has disconnected.");
                        _playerManager.LogOut(p);
                        Account ac;
                        p.MessageService = null;
                        _playerManager.connectionToPlayer.TryRemove(receivedMessage.SenderConnection, out p);
                    }
                    else //Player has been handed off, do not disconnect
                    {
                        //Players are small objects, and RegisterPlayer uses AddOrUpdate, so for now we won't remove them from memory 
                        p.IsHandedOff = false;
                        p.MessageService = null;
                        _playerManager.connectionToPlayer.TryRemove(receivedMessage.SenderConnection, out p);
                    }
                }
            }
        }

        private void HandleConnectionApproval(NetIncomingMessage receivedMessage)
        {
            HailMessages hailMessage = (HailMessages)receivedMessage.ReadByte();
            if (hailMessage == HailMessages.ClientLogin)
            {
                string username = receivedMessage.ReadString();
                ConsoleManager.WriteLine(username + " attempting to connect for login", ConsoleMessageType.Login);
                string password = receivedMessage.ReadString();
                string outMsg;
                Account a;
                //Credentials checked here and on master server to prevent spoofing, where for example the master server processes a valid login and the client sends a different username to the slave
                if (_connectionManager.IsConnectionValid(receivedMessage.SenderConnection, username, password,
                    true, out outMsg, out a))
                {
                    receivedMessage.SenderConnection.Approve();

                    double initTime = TimeKeeper.MsSinceInitialization;

                    while (receivedMessage.SenderConnection.Status != NetConnectionStatus.Connected)
                    {
                        Thread.Sleep(10);
                        if (TimeKeeper.MsSinceInitialization - initTime > 3000)
                            throw new Exception("Client handoff failed: connection timeout on approval attempt.");
                    }

                    ConsoleManager.WriteLine("Client connection approved.");
                    loginManager.HandleLogin(receivedMessage.SenderConnection, a, _chatManager, _simulatorManager,
                        _locatorService);
                }
                else
                {
                    ConsoleManager.WriteLine(outMsg);
                    receivedMessage.SenderConnection.Deny();
                }
            }
            else if (hailMessage == HailMessages.ClientHandoff)
            {
                string username = receivedMessage.ReadString();
                ConsoleManager.WriteLine(username + " attempting to connect for handoff");
                string password = receivedMessage.ReadString();
                string outMsg;
                int shipID;
                int destinationAreaID;
                //Credentials checked here and on master server to prevent spoofing, where for example the master server processes a valid login and the client sends a different username to the slave
                if (_connectionManager.IsConnectionValid(receivedMessage.SenderConnection, username, password,
                    true, out outMsg, out destinationAreaID, out shipID))
                {
                    receivedMessage.SenderConnection.Approve();

                    double initTime = TimeKeeper.MsSinceInitialization;

                    while (receivedMessage.SenderConnection.Status != NetConnectionStatus.Connected)
                    {
                        Thread.Sleep(10);
                        if (TimeKeeper.MsSinceInitialization - initTime > 3000)
                            throw new Exception("Client handoff failed: connection timeout on approval attempt.");
                    }
                    ConsoleManager.WriteLine("Client connection approved.");

                    //playerManager.numOnline++;

                    //Load latest versions of ship and player
                    IShip s = _shipManager.GetShipAsync(shipID, true, _locatorService, true).Result;
                    Player p =
                        _playerManager.GetPlayerAsync((int) s.PlayerID, true, _locatorService, true).Result;
                    
                    p.MessageService = new LidgrenOutgoingMessageService(receivedMessage.SenderConnection,
                        _connectionManager);
                    if (!_playerManager.connectionToPlayer.TryAdd(receivedMessage.SenderConnection, p))
                        throw new Exception("Could not add player to playerManager.connectionToPlayer");
                    //Perform warp
                    //WARNING: Consider implementing energy check                                
                    _warpManager.HandoffCompleteAreaChange(destinationAreaID, s, p);
                }
                else
                {
                    ConsoleManager.WriteLine(outMsg);
                    receivedMessage.SenderConnection.Deny();
                }
            }
        }

        public void ProcessLidgrenMessage(object message)
        {
            var incomingMessage = (NetIncomingMessage)message;

            var messageContainer = PreprocessLidgrenMessage(incomingMessage);
            
            if (messageContainer != null && messageContainer.Message != null)
            {
                //TODO: Check if it's OK/good practice to await inside of ProcessMessage. Make sure doing it like this doesn't bog the server down
                ProcessMessage(this, messageContainer);
            }
        }

        public void ProcessRoutedMessage(object sender, NetworkMessageContainer routedMessage)
        {
            var msg = new ServerNetworkMessage(routedMessage, null);
            ProcessMessage(sender, msg);
        }  
        
        private async Task ProcessMessage(object sender, ServerNetworkMessage serverMessage)
        {
            //TODO: Remove in release? Not sure if this will even work.
            //Make sure this method does not return without stopping the deadlockTimer, otherwise you'll end up with false deadlock detections.
            var deadlockTimer = new DeadlockTimer(Thread.CurrentThread, ServerConfig.MessageProcessingDeadlockTimeout);
            deadlockTimer.Start();


            Interlocked.Increment(ref numMsgHandled);

            var receivedMessage = serverMessage.Message;

            try
            {
                switch (receivedMessage.MessageType)
                {
                        #region Position Update

                    case MessageTypes.PositionUpdateData:
                    {
                        var data = receivedMessage.MessageData as MessagePositionUpdateData;

                        var failedIndices = new List<int>(); //Updates might fail because of concurrency/latency, but should be rare                        

                        for (var i = 0; i < data.UpdateDataObjects.Count; i++)
                        {
                            var ud = data.UpdateDataObjects[i];

                            // Log if we failed to update the ship.
                            if (!_shipManager.UpdateShip(ud))
                            {
                                failedIndices.Add(i);
                            }

                        }

                        //Remove any failed updates
                        for (int i = failedIndices.Count - 1; i >= 0; i--)
                        {
                            data.UpdateDataObjects.RemoveAt(failedIndices[i]);
                        }



                        //Forward successful updates
                        IArea updateArea = _galaxyManager.GetArea(data.AreaID);
                        _clientUpdateManager.ForwardPositionUpdates(data.SendingPlayerID, updateArea, data);


                        break;
                    }

                        #endregion

                        #region Land Request

                    case MessageTypes.LandRequest:
                    {
                        //Consider implementing check to make sure player is near planet to prevent hacking
                        //although such a hack would be rather sophisticated

                        var data = receivedMessage.MessageData as MessageLandRequest;
                        int landingShipID = serverMessage.SendingPlayer == null ? data.LandingShipID : (int) serverMessage.SendingPlayer.ActiveShipId;


                        var landingShip = _shipManager.GetShipAsync(landingShipID).Result;
                        if (landingShip == null)
                        {
                            ConsoleManager.WriteLine("Warning: ship was null during land request. Second request received after handoff?");
                            break;
                        }

                        if (landingShip.CurrentEnergy == landingShip.MaxEnergy)
                        {
                            // Warning: it is technically possible
                            //to land with less than 1000 energy
                            //due to latency. Should be negligible
                            _warpManager.ChangeArea(data.AreaID, landingShip, true, false);

                        }
                        break;
                    }

                        #endregion

                        #region Warp Request

                    case MessageTypes.WarpRequest:
                    {
                        // Do checks here for security
                        //WARNING: Need to implement server side energy check            
                        var data = receivedMessage.MessageData as MessageWarpRequest;
                        Player warpingPlayer = _shipManager.GetShipAsync(data.WarpingShipID).Result.GetPlayer();

                        var currentArea = _galaxyManager.GetArea(warpingPlayer.CurrentAreaID);


                        var warpingShip = warpingPlayer.GetActiveShip();

                        string warpMessage = "";
                        var warpResult = CanWarp(warpingShip, warpingPlayer, currentArea, data.DestinationAreaID, data.WarpholeIndex, out warpMessage);


                        //Debug
                        if (warpResult != WarpAttemptResult.Success && warpResult != WarpAttemptResult.StillInWarpCooldown && warpResult != WarpAttemptResult.NotEnoughEnergy)
                        {
                            Console.WriteLine("Warp failed for user " + warpingPlayer.Username + ": " + warpResult.ToString());
                        }



                        if (warpResult != WarpAttemptResult.Success)
                        {
                            if (warpMessage != "")
                            {
                                NetworkMessageContainer denialMessage = new NetworkMessageContainer(new MessageWarpDenial {DenialMessage = warpMessage}, MessageTypes.WarpDenial);
                                warpingPlayer.SendMessage(denialMessage);
                            }

                            break;
                        }


                        _warpManager.ChangeArea(data.DestinationAreaID, warpingShip, true, true);



                        warpingShip.LastWarpTime = TimeKeeper.MsSinceInitialization;
                    }

                        break;

                        #endregion

                        #region LeaveToPlanetRequest

                    case MessageTypes.LeaveToPlanetRequest:
                    {
                        var data = receivedMessage.MessageData as MessageEmptyMessage;

                        Player tempPlayer = _shipManager.GetShipAsync(data.TargetShipID).Result.GetPlayer();

                        var CurrentArea = _galaxyManager.GetArea(tempPlayer.CurrentAreaID);

                        if (CurrentArea is Colony)
                        {
                            Colony p = (Colony) CurrentArea;
                            int destinationAreaID = p.WarpholeToPlanet.DestinationAreaID;
                            _warpManager.ChangeArea(destinationAreaID, tempPlayer.GetActiveShip(), false, false);

                        }
                        else
                        {
                            ConsoleManager.WriteLine("Error: Recieved LeaveToPlanetRequest from a player who was not in a colony.");
                        }

                    }
                        break;

                        #endregion

                        #region LeaveToSpaceRequest

                    case MessageTypes.LeaveToSpaceRequest:
                    {
                        var data = receivedMessage.MessageData as MessageEmptyMessage;

                        Player tempPlayer = _shipManager.GetShipAsync(data.TargetShipID).Result.GetPlayer();
                        var CurrentArea = _galaxyManager.GetArea(tempPlayer.CurrentAreaID);

                        if (CurrentArea is Colony)
                        {
                            Colony p = (Colony) CurrentArea;
                            int destinationAreaID = p.WarpholeToSpace.DestinationAreaID;
                            _warpManager.ChangeArea(destinationAreaID, tempPlayer.GetActiveShip(), false, true);

                        }
                        else
                        {
                            ConsoleManager.WriteLine("Error: Recieved LeaveToSpaceRequest from a player who was not in a colony.");
                        }

                    }
                        break;

                        #endregion

                        #region Dome Entry/Capture Attempt

                    case MessageTypes.EnterColonyRequest:
                    {

                        var data = receivedMessage.MessageData as MessageEmptyMessage;

                        Player enteringPlayer = _shipManager.GetShipAsync(data.TargetShipID).Result.GetPlayer();
                        var CurrentArea = _galaxyManager.GetArea(enteringPlayer.CurrentAreaID);

                        if (!(CurrentArea is Planet))
                        {
                            ConsoleManager.WriteLine("ERROR: " + MessageTypes.EnterColony + " message recieved from a player who was not on a planet.", ConsoleMessageType.Error);
                        }
                        else if (((Planet) CurrentArea).GetColony() != null)
                        {
                            Planet pl = (Planet) CurrentArea;

                            //Hack check
                            if (enteringPlayer.GetActiveShip().GetDistance(pl.GetColony().CommandCenter) > 1)
                            {

                                ConsoleManager.WriteLine("Ship attempted dome cap when not near dome. Hack attempt?", ConsoleMessageType.Error);
#if !DEBUG
                                break;//Don't send succesful response in release
#endif
                            }


                            if (enteringPlayer.GetTeamIDs().Contains((int) pl.GetColony().OwnerDefaultTeamID)) //Enter without capping and with any energy amount if on same team as owner
                            {
                                _warpManager.ChangeArea((int) pl.ColonyID, enteringPlayer.GetActiveShip(), false, false);
                            }
                            else if (enteringPlayer.GetActiveShip().CurrentEnergy == enteringPlayer.GetActiveShip().ShipStats.Energy) //If energy is full, allow capture
                            {
                                Player prevOwner = pl.GetOwner();
                                pl.GetColony().ChangeOwner(enteringPlayer);
                                _warpManager.ChangeArea((int) pl.ColonyID, enteringPlayer.GetActiveShip(), false, false);

                                var capturingPlayerChat = new OutboundChatMessage(
                                    enteringPlayer,
                                    new ChatlineObject(
                                        "The Colony '" + pl.GetColony().Name + "' in '" + pl.GetParentArea().AreaName
                                        + "' was captured by " + enteringPlayer.Username + "!",
                                        ChatlineColor.White
                                    )
                                );

                                _chatManager.SendChatToPlayer(capturingPlayerChat);

                                if (prevOwner.IsOnline)
                                {

                                    var previousOwnerChat = new OutboundChatMessage(
                                        prevOwner,
                                        new ChatlineObject(
                                            "The Colony " + pl.GetColony().Name + " in " + pl.GetParentArea().AreaName
                                            + " was captured by " + enteringPlayer.Username + "!",
                                            ChatlineColor.White
                                        )
                                    );

                                    _chatManager.SendChatToPlayer(previousOwnerChat);
                                }

                                //Boot all non allied players to system
                                foreach (var s in pl.GetShips())
                                {
                                    if (!_teamManager.AreAllied(s.Value, enteringPlayer))
                                    {
                                        _warpManager.ChangeArea((int) CurrentArea.ParentAreaID, s.Value, false, true);
                                    }

                                }
                                pl.SendCaptureMessage();


                            }


                        }


                        break;
                    }



                        #endregion

                        #region Chat Message

                    case MessageTypes.ChatMessage:
                        _chatManager.HandleMessage(receivedMessage, serverMessage.SendingPlayer);

                        break;

                        #endregion

                        #region Ship Fire Request

                    case MessageTypes.ShipFireRequest:
                    {
                        var data = receivedMessage.MessageData as MessageShipFireRequest;

                        IShip firingShip = _shipManager.GetShipAsync(data.ShipID).Result;
                        var weapon = firingShip.GetWeapon(data.WeaponSlot);

                        if (firingShip == null)
                        {
                            ConsoleManager.WriteLine("Error: ship not found while processing ShipFireRequest message.", ConsoleMessageType.Error);
                            break;
                        }

                        if (weapon == null)
                        {
                            ConsoleManager.WriteLine("Warning: invalid slot received during ShipFireRequest.", ConsoleMessageType.Warning);
                            break;
                        }

                        if (weapon.Stats.WeaponType == WeaponTypes.MissileLauncher)
                        {
                            var missileLauncher = weapon as MissileLauncher;
                            missileLauncher.SetMissileType(data.ProjectileType);
                        }

                        if (weapon.CanFire(firingShip))
                        {
                            weapon.Fire(firingShip);
                            _galaxyManager.GetArea(firingShip.CurrentAreaId).ShipFired(
                                firingShip, data.Rotation, data.WeaponSlot, data.ProjectileIDs, _projectileManager,
                                data.PctCharge);

                            //MissileLauncher might need its own class for this
                            if (weapon.Stats.WeaponType == WeaponTypes.MissileLauncher)
                            {
                                _cargoSynchronizer.RequestTransaction(new TransactionRemoveStatelessCargo(firingShip, ((MissileLauncherStats) weapon.Stats).MissileType, weapon.Stats.NumProjectiles));

                                var msgData = new MessageRemoveCargoFromShip {ShipID = data.ShipID};
                                msgData.StatelessCargoData.Add(new StatelessCargoData() {CargoType = ((MissileLauncherStats) weapon.Stats).MissileType});
                                firingShip.GetPlayer().SendMessage(new NetworkMessageContainer(msgData, MessageTypes.RemoveCargoFromShip));

                                MessageFireRequestResponse response = new MessageFireRequestResponse
                                {
                                    FiringObjectType = FiringObjectTypes.Ship,
                                    FiringObjectID = data.ShipID,
                                    Approved = true,
                                    WeaponSlot = data.WeaponSlot,
                                    NumProjectiles = weapon.Stats.NumProjectiles
                                };
                                firingShip.GetPlayer().SendMessage(new NetworkMessageContainer(response, MessageTypes.FireRequestResponse));
                            }
                        }
                        else
                        {
                            //TODO: Consider disabling this, may be ok to silently deny fire requests
                            MessageFireRequestResponse response = new MessageFireRequestResponse
                            {
                                FiringObjectType = FiringObjectTypes.Ship,
                                FiringObjectID = data.ShipID,
                                Approved = false,
                                WeaponSlot = data.WeaponSlot,
                                NumProjectiles = firingShip.GetWeapon(data.WeaponSlot).Stats.NumProjectiles
                            };
                            firingShip.GetPlayer().SendMessage(new NetworkMessageContainer(response, MessageTypes.FireRequestResponse));

                        }

                        break;


                    }

                        #endregion

                        #region Structure Fire Request

                    case MessageTypes.StructureFireRequest:
                    {
                        var data = receivedMessage.MessageData as MessageStructureFireRequest;

                        IStructure s = _structureManager.GetObjectAsync(data.StructureID).Result;
                        IArea a = _galaxyManager.GetArea(s.CurrentAreaId);


                        if (s == null)
                        {
#if DEBUG
                            ConsoleManager.WriteLine("Warning: structure not found on StructureFireRequest. Sometimes occurs if client changes area while structures are firing, or a request is received after a structure is killed.", ConsoleMessageType.Warning);
#endif

                            break;
                        }

                        if (!a.TryFireStructure(data.StructureID, data.Rotation, data.ProjectileIDs, _projectileManager, data.PctCharge, data.WeaponSlot))
                        {
                            MessageFireRequestResponse response = new MessageFireRequestResponse
                            {
                                FiringObjectType = FiringObjectTypes.Structure,
                                FiringObjectID = data.StructureID,
                                Approved = false,
                                WeaponSlot = data.WeaponSlot,
                                NumProjectiles = s.Weapon.Stats.NumProjectiles,

                            };

                            _simulatorManager.SendMessageToSimulator(new SimulatorBoundMessage(response, MessageTypes.FireRequestResponse, a.Id));
                        }


                        break;
                    }

                        #endregion

                        #region Collision Report

                    case (MessageTypes.ProjectileCollisionReport):
                    {
                        var data = receivedMessage.MessageData as MessageProjectileCollisionReport;

                        foreach (var cr in data.Collisions)
                        {
                            _collisionManager.CreateCollision(cr.ProjectileID, cr.ProjectileType, cr.HitObjectID, cr.PctCharge, cr.WeaponSlot);
                        }


                        break;
                    }

                        #endregion

                        #region ObjectPickupRequest

                    case MessageTypes.ObjectPickupRequest:
                    {
                        var data = receivedMessage.MessageData as MessageObjectPickupRequest;

                        IShip s = _shipManager.GetShipAsync(data.RequestingShipID).Result;
                        if (s == null)
                        {
                            break;
                        }
                        IArea currentArea = _galaxyManager.GetArea(s.CurrentAreaId);



                        switch (data.ObjectType)
                        {
                            case PickupableTypes.FloatyAreaObject:
                            {

                                IFloatyAreaObject obj = currentArea.GetFloatyAreaObject(data.ObjectID);
                                if (!CanPickup(s, obj))
                                    break;


                                switch (obj.FloatyType)
                                {
                                    case FloatyAreaObjectTypes.Module:
                                    {
                                        Module m = (Module) obj;
                                        if (s.Cargo.CheckCargoSpace(m.CargoType, 1))
                                        {

                                            var ct = new TransactionAddStatefulCargo(s, m, false);
                                            ct.OnCompletion += s.CargoAdded;
                                            ct.OnCompletion += _messageManager.NotifyCargoAdded;
                                            _cargoSynchronizer.RequestTransaction(ct);
                                            await ct.ResultTask;
                                            if (ct.ResultTask.Result == CargoResult.Success)
                                            {
                                                currentArea.RemoveFloatyAreaObjects(new HashSet<int> {m.Id}); //TODO: refactor and add method that matches even handler signature so that object is only removed via succesful transaction
                                            }

                                        }


                                        break;
                                    }

                                }
                            }
                                break;

                            case PickupableTypes.DefensiveMine:
                            {
                                var obj = currentArea.GetStructure(data.ObjectID);
                                if (!CanPickup(s, obj) || !s.Cargo.CheckCargoSpace(StatefulCargoTypes.DefensiveMine, 1))
                                    break;


                                var ct = new TransactionAddStatefulCargo(s, new StatefulCargo(data.ObjectID, StatefulCargoTypes.DefensiveMine), true);
                                _cargoSynchronizer.RequestTransaction(ct);
                                await ct.ResultTask;
                                if (ct.ResultTask.Result == CargoResult.Success)
                                {
                                    currentArea.RemoveStructure(obj.Id, true); //TODO: refactor and add method that matches even handler signature so that object is only removed via succesful transaction
                                }

                                break;
                            }
                            case PickupableTypes.Turret:
                            {
                                var obj = currentArea.GetStructure(data.ObjectID);
                                if (!CanPickup(s, obj) || !s.Cargo.CheckCargoSpace(StatefulCargoTypes.LaserTurret, 1))
                                    break;

                                var ct = new TransactionAddStatefulCargo(s, new CargoLaserTurret(obj.Id, obj.CurrentHealth, new LaserWeaponStats()), true);
                                _cargoSynchronizer.RequestTransaction(ct);
                                await ct.ResultTask;
                                if (ct.ResultTask.Result == CargoResult.Success)
                                {
                                    currentArea.RemoveStructure(obj.Id, true); //TODO: refactor and add method that matches even handler signature so that object is only removed via succesful transaction
                                    ConsoleManager.WriteLine("Picked up turret", ConsoleMessageType.Debug);
                                }
#if DEBUG
                                else
                                {
                                    ConsoleManager.WriteLine(ct.ResultTask.ToString(), ConsoleMessageType.Debug);
                                }
#endif

                                break;


                            }

                        }


                        break;
                    }

                        #endregion

                        #region Dock Request

                    case MessageTypes.DockRequest:
                    {
                        ConsoleManager.WriteLine(receivedMessage.MessageType + " processing not implemented.", ConsoleMessageType.Warning);

                        //var data = SerializationUtilities.DeserializeMsgPack<MessageDockRequest>(receivedMessage);

                        //var ship = _shipManager.GetShipAsync(data.DockingShipID).Result;

                        //var hasFullEnergy = ship.CurrentEnergy == (ship.ShipStats.Energy + ship.StatBonuses[StatBonusTypes.MaxEnergy]);

                        //var port = _galaxyManager.GetArea(data.PortID); //Get port's area ID
                        //if (!hasFullEnergy || port.AreaType != AreaTypes.Port || ship.GetDistance(port) > 1)
                        //    break; //If the ship doesnt have full energy, don't change the area.

                        //_warpManager.ChangeArea(port.Id, ship, false);


                        break;
                    }

                        #endregion

                        #region Undock Request

                    case MessageTypes.UndockRequest:
                    {
                        ConsoleManager.WriteLine(receivedMessage.MessageType + " processing not implemented.", ConsoleMessageType.Warning);

                        //int tempShipID = receivedMessage.ReadInt32();
                        //IShip s = _shipManager.GetShipAsync(tempShipID).Result;
                        //var a = _galaxyManager.GetArea(s.CurrentAreaId);
                        //if (a.AreaType == AreaTypes.Port)
                        //    _warpManager.ChangeArea(a.ParentAreaID, _shipManager.GetShipAsync(tempShipID).Result, false);
                        //else
                        //    ConsoleManager.WriteLine("Warning: UndockRequest recieved from " +
                        //                                   _shipManager.GetShipAsync(tempShipID).Result.GetPlayer().Username +
                        //                                   " while he was not docked.");
                        break;
                    }

                        #endregion

                        #region PortTradeRequest

                    case MessageTypes.PortTradeRequest:
                    {
                        var ship = serverMessage.SendingPlayer.GetActiveShip();
                        var port = (Port) ship.GetArea();
                        var response = new MessagePortTradeResponse();

                        if (port == null)
                        {
                            response.ResponseMessage = "Ship not in port.";
                        }
                        else
                        {

                            var data = receivedMessage.MessageData as MessagePortTradeRequest;
                            PurchaseResult result = await _economyManager.TryTradeWithPort(port, ship, data.TradeDirection, data.TypesAndQuantities, data.StatefulCargoIDs);
                            response.ResponseMessage = result.ToString().SplitCamelCase();
                        }

                        response.ShipCargo = ship.Cargo.GetNetworkData();
                        serverMessage.SendingPlayer.SendMessage(new NetworkMessageContainer(response, MessageTypes.PortTradeResponse));

                        break;
                    }


                        #endregion

                        #region ShipTradeRequest

                    case MessageTypes.ShipTradeRequest:
                    {
                        if (serverMessage.SendingPlayer == null)
                            throw new Exception("Trade request received, but player is null.");

                        var data = serverMessage.Message.MessageData as MessageShipTradeRequest;
                        var requestingShip = serverMessage.SendingPlayer.GetActiveShip();
                        var targetPlayer = _playerManager.GetPlayer(data.TargetPlayerUsername);
                        if (targetPlayer == null) //Either non-local name or doesn't exist
                        {
                            string responseText = "";

                            targetPlayer = await _playerManager.GetPlayerAsync(data.TargetPlayerUsername, true, _locatorService, false);
                            if (targetPlayer == null) //Player doesn't exist
                            {
                                responseText = "No response received from " + data.TargetPlayerUsername + ".";
                            }
                            else //We'll be cheap for now and require that trades happen between ships on the same server
                            {
                                responseText = "Captain, something appears to be attenuating " + targetPlayer.Username + "'s response. We must warp closer to the target vessel to initiate a trade.";
                            }

                            _chatManager.SendSimpleChat(serverMessage.SendingPlayer, "[COMM]", responseText, ChatTypes.Notification);

                            break;
                        }

                        //targetPlayer found and is local
                        var targetShip = targetPlayer.GetActiveShip();

                        if (requestingShip == null || targetShip == null)
                        {
                            break;
                        }

                        var result = _economyManager.ProcessTradeRequest(requestingShip, targetShip);

                        if (result == TradeResult.TradeInitialized)
                        {
                            serverMessage.SendingPlayer.SendMessage(new NetworkMessageContainer(new MessageInitiateShipTrade() {CurrentCargoData = requestingShip.Cargo.GetNetworkData(), TargetUsername = targetPlayer.Username}, MessageTypes.InitiateShipTrade));
                            targetPlayer.SendMessage(new NetworkMessageContainer(new MessageInitiateShipTrade() {CurrentCargoData = targetShip.Cargo.GetNetworkData(), TargetUsername = serverMessage.SendingPlayer.Username}, MessageTypes.InitiateShipTrade));
                        }


                        break;
                    }


                        #endregion

                        #region CancelShipTrade

                    case MessageTypes.CancelShipTrade:
                    {
                        if (serverMessage.SendingPlayer == null)
                            throw new InvalidProgramException(receivedMessage.MessageType.ToString() + " received, but sendingPlayer is null.");

                        if (serverMessage.SendingPlayer.ActiveShipId == null)
                            break;

                        _economyManager.TerminateTrade((int) serverMessage.SendingPlayer.ActiveShipId, true);

                        break;

                    }

                        #endregion

                        #region TradeUpdateData

                    case MessageTypes.TradeUpdateData:
                    {
                        if (serverMessage.SendingPlayer == null)
                            throw new InvalidProgramException(receivedMessage.MessageType.ToString() + " received, but sendingPlayer is null.");

                        var data = serverMessage.Message.MessageData as MessageTradeUpdateData;
                        var tradeData = _economyManager.UpdateTradeData(data.TradeData);

                        if (tradeData != null) //Update succesful, push to clients
                        {
                            var responseMessage = new MessageShipTradeData() {TradeUIData = UIHelper.GetUIData(tradeData), TradeData = tradeData.GetNetworkData()};
                            tradeData.ShipA.GetPlayer().SendMessage(new NetworkMessageContainer(responseMessage, MessageTypes.TradeUpdateData));
                            tradeData.ShipB.GetPlayer().SendMessage(new NetworkMessageContainer(responseMessage, MessageTypes.TradeUpdateData));
                        }

                        break;
                    }

                        #endregion

                        #region Structure Placement Request

                    case MessageTypes.StructurePlacementRequest:
                    {
                        var requestData = receivedMessage.MessageData as MessageStructurePlacementRequest;

                        var requestingShip = _shipManager.GetShipAsync(requestData.RequestingShipID).Result;

                        Player requestingPlayer = requestingShip.GetPlayer();
                        if (requestingPlayer == null)
                        {
                            ConsoleManager.WriteLine("Player connection not found while processing StructurePlacementRequest message.", ConsoleMessageType.Warning);
                            break;
                        }

                        switch (requestData.StructureType)
                        {
                            case StructureTypes.Biodome:
                            {
                                var a = _galaxyManager.GetArea(requestingPlayer.CurrentAreaID);

                                string resultMessage = "";

                                if (a is Planet && _galaxyManager.TryColonizePlanet((Planet) a, requestingShip, _warpManager, _locatorService, requestData.PosX, requestData.PosY, out resultMessage, _databaseManager))
                                {
//If player is on a planet and colonization is successful
                                    _cargoSynchronizer.RequestTransaction(new TransactionRemoveStatelessCargo(requestingShip, StatelessCargoTypes.Biodome, 1));
                                    var response = new MessageColonizeRequestApproval();
                                    response.ColonyStructure = (CommandCenterData) ((Planet) a).GetColony().CommandCenter.GetNetworkData();

                                    requestingPlayer.SendMessage(new NetworkMessageContainer(response, MessageTypes.ColonizeRequestApproval));

                                }
                                else
                                {
                                    //if colonization is not successful

                                    var response = new MessageColonizeRequestDenial();
                                    response.DenialMessage = resultMessage;
                                    requestingPlayer.SendMessage(new NetworkMessageContainer(response, MessageTypes.ColonizeRequestDenial));
                                }



                                break;
                            }
                            case StructureTypes.LaserTurret:
                            {


                                IShip ship = _shipManager.GetShipAsync((int) requestingPlayer.ActiveShipId).Result;
                                if (!ship.Cargo.IsCargoInHolds(StatefulCargoTypes.LaserTurret, 1))
                                    break;

                                int cargoID = requestData.CargoID == null ? ship.Cargo.GetAnyStatefulCargo(StatefulCargoTypes.LaserTurret).Id : (int) requestData.CargoID;


                                var playerCurrentArea = _galaxyManager.GetArea(requestingPlayer.CurrentAreaID);
                                float xPos = requestData.PosX;
                                float yPos = requestData.PosY;
                                string resultMessage = "";
                                bool success = false;


                                if (playerCurrentArea == null)
                                {
                                    resultMessage = "Player area not found.";
                                    ConsoleManager.WriteLine("Player are not found during laser turret placement request.", ConsoleMessageType.Error);
                                    success = false;
                                }
                                else if (PlanetTurretPlacementRequest(playerCurrentArea, requestingPlayer, cargoID, ref xPos, ref yPos, out resultMessage))
                                {
                                    TransactionRemoveStatefulCargo t = new TransactionRemoveStatefulCargo(ship, StatefulCargoTypes.LaserTurret, cargoID, false);
                                    _cargoSynchronizer.RequestTransaction(t);
                                    await t.ResultTask;
                                    if (t.ResultTask.Result == CargoResult.Success)
                                    {

                                        Planet p = (Planet) playerCurrentArea;
                                        Colony c = p.GetColony();
                                        IStructure s = StructureFactory.CreateStructure(t.RemovedCargo, xPos, yPos, requestingPlayer, c.CommandCenter, playerCurrentArea.Id, _playerManager);
                                        ((Turret) s).IsOnPlanet = true; //Temporary
                                        p.AddStructure(s);
                                        var removeMessage = new MessageRemoveCargoFromShip {ShipID = ship.Id};
                                        removeMessage.StatefulCargoIDs.Add((int) t.CargoID);
                                        ship.GetPlayer().SendMessage(new NetworkMessageContainer(removeMessage, MessageTypes.RemoveCargoFromShip));


                                    }
                                    else
                                    {
                                        resultMessage = "Could not remove cargo from ship.";
                                        success = false;

                                    }

                                }
                                else if (SystemTurretPlacementRequest(playerCurrentArea, requestingPlayer, cargoID, ref xPos, ref yPos, out resultMessage))
                                {

                                    TransactionRemoveStatefulCargo t = new TransactionRemoveStatefulCargo(ship, StatefulCargoTypes.LaserTurret, cargoID, false);
                                    _cargoSynchronizer.RequestTransaction(t);
                                    await t.ResultTask;
                                    if (t.ResultTask.Result == CargoResult.Success)
                                    {
                                        IStructure s = StructureFactory.CreateStructure(t.RemovedCargo, xPos, yPos, requestingPlayer, null, playerCurrentArea.Id, _playerManager);
                                        playerCurrentArea.AddStructure(s);
                                        _databaseManager.SaveAsync(playerCurrentArea);
                                        var removeMessage = new MessageRemoveCargoFromShip {ShipID = ship.Id};
                                        removeMessage.StatefulCargoIDs.Add((int) t.CargoID);
                                        ship.GetPlayer().SendMessage(new NetworkMessageContainer(removeMessage, MessageTypes.RemoveCargoFromShip));
                                    }
                                    else
                                    {
                                        success = false;
                                    }
                                }
                                else
                                {
                                    success = false;
                                }

                                if (!success)
                                {
                                    var response = new MessageStructureRequestResponse
                                    {
                                        Approved = false,
                                        Message = resultMessage
                                    };
                                    requestingPlayer.SendMessage(new NetworkMessageContainer(response, MessageTypes.StructurePlacementResponse));
                                }


                                break;
                            }
                            case StructureTypes.DefensiveMine:
                            {

                                IShip ship = _shipManager.GetShipAsync((int) requestingPlayer.ActiveShipId).Result;

                                var playerCurrentArea = _galaxyManager.GetArea(requestingPlayer.CurrentAreaID);
                                float xPos = requestData.PosX;
                                float yPos = requestData.PosY;
                                string resultMessage = "";
                                bool success = false;

                                if (!ship.Cargo.IsCargoInHolds(StatefulCargoTypes.DefensiveMine, 1))
                                    break;

                                int cargoID = requestData.CargoID == null ? ship.Cargo.GetAnyStatefulCargo(StatefulCargoTypes.DefensiveMine).Id : (int) requestData.CargoID;

                                if (playerCurrentArea == null)
                                {
                                    resultMessage = "Player area not found.";
                                    ConsoleManager.WriteLine("Player are not found during mine placement request.", ConsoleMessageType.Error);
                                    success = false;
                                }
                                else if (MinePlacementRequest(playerCurrentArea, requestingPlayer, cargoID, ref xPos, ref yPos, out resultMessage))
                                {

                                    TransactionRemoveStatefulCargo t = new TransactionRemoveStatefulCargo(ship, StatefulCargoTypes.DefensiveMine, cargoID, false);
                                    _cargoSynchronizer.RequestTransaction(t);
                                    await t.ResultTask;
                                    if (t.ResultTask.Result == CargoResult.Success)
                                    {
                                        IStructure s = StructureFactory.CreateStructure(t.RemovedCargo, xPos, yPos, requestingPlayer, null, playerCurrentArea.Id, _playerManager);
                                        playerCurrentArea.AddStructure(s);
                                        _databaseManager.SaveAsync(playerCurrentArea);
                                        //"Response" comes in the form of an add structure message
                                    }
                                    else
                                    {
                                        success = false;
                                    }
                                }
                                else
                                {
                                    success = false;
                                }

                                if (!success)
                                {
                                    var response = new MessageStructureRequestResponse
                                    {
                                        Approved = false,
                                        Message = resultMessage
                                    };
                                    requestingPlayer.SendMessage(new NetworkMessageContainer(response, MessageTypes.StructurePlacementResponse));
                                }


                                break;
                            }

                        }
                        break;
                    }

                        #endregion

                        #region Client Exception

                    case MessageTypes.ClientException:
                    {
                        ConsoleManager.WriteLine("Client Exception received, transmission of exceptions is not fully implemented.", ConsoleMessageType.Warning);
                    }
                        break;

                        #endregion

                        #region Cheat Detected

                    case MessageTypes.CheatNotification:
                        ConsoleManager.WriteLine("Cheat notifications disabled.", ConsoleMessageType.Warning);
                        break;
                        //string cheatName = "";
                        //try
                        //{
                        //    cheatName = receivedMessage.ReadString();
                        //}
                        //catch
                        //{
                        //    ConsoleManager.WriteLine("Error Reading Cheat");
                        //    break;
                        //}
                        //// If a player is on the login screen
                        //if (!_playerManager.connectionToPlayer.ContainsKey(receivedMessage.SenderConnection))
                        //    break;

                        //Player hacker = _playerManager.connectionToPlayer[receivedMessage.SenderConnection];
                        //hacker.CheatEngineOpen = true;
                        //ConsoleManager.WriteLine("Cheat Engine on Player " +
                        //                               hacker.Username +
                        //                               ", " + cheatName);
                        //foreach (var p in _playerManager.Players)
                        //{
                        //    if (p.GetAccount().IsAdmin && p.IsOnline)
                        //        _messageManager.SendChatMessage("Hack Report: ", hacker.Username + " using " + cheatName,
                        //                                       ChatTypes.admin, (HumanPlayer)p);


                        //}
                        //break;

                        #endregion

                        #region TimeGet

                    case MessageTypes.TimeGet:
                        try
                        {
                            var data = receivedMessage.MessageData as MessageTimeSync;
                            // Syncs time on login, or whenever we decide to do a resync
                            _playerManager.GetPlayerAsync(data.PlayerID).Result.ClientTime = TimeSpan.FromMilliseconds(data.TimeMS);
                        }
                        catch
                        {
                            ConsoleManager.WriteLine("TimeGet Error");
                        }
                        break;

                        #endregion

                        #region SelectorCommandMessage

                    case MessageTypes.SelectorMessageType:
                    {
                        var data = receivedMessage.MessageData as MessageSelectorCommand;
                        _simulatorManager.SendMessageToSimulator(new SimulatorBoundMessage(data, MessageTypes.SelectorMessageType, data.AreaID));

                        break;
                    }

                        #endregion

                    default:
                        ConsoleManager.WriteLine("Unhandled message type received: " + receivedMessage.MessageType);
                        break;
                }

            }
            catch (Exception e)
            {
                //Get innermost exception
                while (e.InnerException != null)
                {
                    e = e.InnerException;
                }
                ConsoleManager.WriteLine(e.ToString(), ConsoleMessageType.Error);

                //exceptionLogger.Write(e.ToString());
                //exceptionLogger.Write("\n");
                //exceptionLogger.Flush();
                

            }
            finally
            {
                deadlockTimer.Stop();
            }
        }

        





    }
}
