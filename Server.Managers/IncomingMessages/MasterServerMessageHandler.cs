using Server.Database;
using Server.Models;
using SRServer.Services;
using System.Diagnostics;
using System;
using System.Threading.Tasks;
using Server.MongoDB;
using Freecon.Models.TypeEnums;
using Freecon.Core.Networking.ServerToServer;
using Core.Models.Enums;
using Freecon.Core.Networking.Models.ServerToServer;
using RedisWrapper;
using Server.Interfaces;
using Freecon.Core.Networking.Models;
using Core.Cryptography;

namespace Server.Managers.IncomingMessages
{
    /// <summary>
    /// Incoming messages from the master server to this slave
    /// </summary>
    public class MasterServerMessageHandler
    {
        ConnectionManager _connectionManager;
        AccountManager _accountManager;
        LocalIDManager _accountIdManager;
        LocalIDManager _galaxyIDManager;
        LocalIDManager _teamIDManager;
        LocalIDManager _transactionIdManager;
        GalaxyManager _galaxyManager;
        PlayerManager _playerManager;
        ShipManager _shipManager;
        GalaxyRegistrationManager _registrationManager;
        GlobalTeamManager _teamManager;
        IDatabaseManager _databaseManager;
        LocatorService _locatorService;
        MessageManager _messageManager;
        RedisServer _redisServer;
        WarpManager _warpManager;

        EventHandler<NetworkMessageContainer> _routedMessageProcessor;

        int _mySlaveID;

        public bool PSystemsLoaded;//Signals that all PSystems have been loaded and the server is ready
        //TODO: Implement pause to object updating when additional systems need to be loaded while server is up

        public MasterServerMessageHandler(
            int mySlaveID,
            RedisServer redisServer,
            ConnectionManager connectionManager,
            LocatorService ls,
            AccountManager accountManager,
            LocalIDManager accountIdManager,
            IDatabaseManager databaseManager,
            GalaxyManager galaxyManager,
            LocalIDManager galaxyIDManager,
            PlayerManager playerManager,
            ShipManager shipManager,
            GalaxyRegistrationManager rm,
            LocalIDManager teamIDManager,
            MessageManager messageManager,
            GlobalTeamManager teamManager,
            WarpManager warpManager,
            LocalIDManager transactionIdManager,
            EventHandler<NetworkMessageContainer> routedMessageProcessor 
        )
        {
            _connectionManager = connectionManager;
            _accountManager = accountManager;
            _accountIdManager = accountIdManager;
            _databaseManager = databaseManager;
            _galaxyIDManager = galaxyIDManager;
            _galaxyManager = galaxyManager;
            _teamIDManager = teamIDManager;
            _playerManager = playerManager;
            _shipManager = shipManager;
            _registrationManager = rm;
            _locatorService = ls;
            _messageManager = messageManager;
            _teamManager = teamManager;
            _redisServer = redisServer;
            _warpManager = warpManager;
            _transactionIdManager = transactionIdManager;

            _mySlaveID = mySlaveID;

            _routedMessageProcessor = routedMessageProcessor;
            
            _redisServer.Subscribe(MessageTypes.Redis_LoginDataRequest, _handleLoginDataRequest);
            _redisServer.Subscribe(MessageTypes.Redis_ColonyDataPush, _handleColonyDataPush);
            _redisServer.Subscribe(MessageTypes.Redis_ClientHandoff, _handleHandoff);
            _redisServer.Subscribe(MessageTypes.Redis_StartUpdatingSystems, _handleMessageStartUpdatingSystems);
            _redisServer.Subscribe(MessageTypes.Redis_IDResponse, _handleMessageIDResponse);
            _redisServer.Subscribe(MessageTypes.Redis_AdminWarpPlayer, _handleAdminWarpPlayer);
        }

        private void _handleHandoff(object sender, NetworkMessageContainer messageData)
        {
            MessageClientHandoff c = messageData?.MessageData as MessageClientHandoff;

            if (_galaxyManager.IsLocalArea(c.DestinationAreaID))
            {
                ConsoleManager.WriteLine("Received client handoff");

                Account refreshedAccount = _accountManager.GetAccountAsync(c.AccountID, true, true).Result;
                _connectionManager.AddPendingHandoff(c.IPAddress, refreshedAccount, c.DestinationAreaID, c.ShipID, c.ServerGameStateId);
            }
        }

        private void _handleMessageIDResponse(object sender, NetworkMessageContainer messageData)
        {
            var data = messageData?.MessageData as MessageIDResponse;
            if (data.SlaveServerID != _mySlaveID)
            {
                return;
            }
            else
            {
                switch (data.IDType)
                {
                    case IDTypes.GalaxyID:
                        _galaxyIDManager.ReceiveServerIDs(data.IDs);
                        break;

                    case IDTypes.TeamID:
                        _teamIDManager.ReceiveServerIDs(data.IDs);
                        break;
                    case IDTypes.AccountID:
                        _accountIdManager.ReceiveServerIDs(data.IDs);
                        break;

                    case IDTypes.TransactionID:
                        _transactionIdManager.ReceiveServerIDs(data.IDs);
                        break;

                    default:
                        throw new NotImplementedException();
                        break;

                }
            }
        }

        private void _handleDataPush(MessageColonyDataPush data, Colony colony, IShip requestingShip)
        {
            switch (data.UpdateType)
            {
                case UpdateTypes.SetSlider:
                    {
                        SliderTypes sliderType = (SliderTypes)(byte)data.FirstIdentifier;//Dunno if the byte cast is necessary
                        float sliderValue = data.Data;
                        colony.Sliders[sliderType].CurrentValue = sliderValue;

                        break;
                    }

                case UpdateTypes.AddConstructableToQueue:
                    {
                        throw new NotImplementedException();
                        break;
                    }

                case UpdateTypes.WithdrawResource:
                    {
                        throw new NotImplementedException();
                        //TransactionSequence tr = new TransactionSequence();
                        //tr.AddTransaction

                        //_cargoSynchronizer.RequestAtomicTransactionSequence()
                        break;
                    }

                case UpdateTypes.DropResource:
                    {
                        throw new NotImplementedException();
                        break;
                    }


            }
        }

        private void _handleMessageStartUpdatingSystems(object sender, NetworkMessageContainer messageData)
        {           
            var data = messageData?.MessageData as MessageStartUpdatingSystems;

            // Return if this isn't the target slave
            if (data.SlaveServerID != _mySlaveID)
            {
                return;
            }

            PSystemsLoaded = false;

            if (data.ClearCurrentSystems)
            {
                _galaxyManager.ClearLocalSystems();
            }

#if DEBUG
            Stopwatch s1 = new Stopwatch();
            s1.Start();
#endif

            var loadedSystemModels = _databaseManager.GetAreasAsync(data.IDsToSimulate).Result;

            foreach (PSystemModel p in loadedSystemModels)
            {
                PSystem s = Deserializer.DeserializePSystemAsync(p, _redisServer, _locatorService, _registrationManager, _databaseManager).Result;
            }

            
            foreach (var a in _galaxyManager.AllAreas)
            {
                //Clear existing subscriptions to prevent doubles (for hot loading)
                _redisServer.UnSubscribe(ChannelTypes.WebToSlave, a.Value.Id, _routedMessageProcessor);

                //Subscribe
                _redisServer.Subscribe(ChannelTypes.WebToSlave, a.Value.Id, _routedMessageProcessor);
            }

#if DEBUG

            ConsoleManager.WriteLine(_galaxyManager.AllAreas.Count.ToString() + " total areas currently loaded.", ConsoleMessageType.Debug);
            s1.Stop();
            ConsoleManager.WriteLine(s1.ElapsedMilliseconds + " ms to load " + _galaxyManager.Systems.Count + " systems.", ConsoleMessageType.Debug);
#endif
            
            PSystemsLoaded = true;
        }

        /// <summary>
        /// Handles redis request for login data.
        /// </summary>
        private void _handleLoginDataRequest(object sender, NetworkMessageContainer messageData)
        {
            var req = messageData?.MessageData as ClientLoginDataRequest;

            if (!_galaxyManager.IsLocalSystem(req.LastSystemID))
            {
                return;
            }

            _handleLoginDataRequestAsync(req);
        }

        private async Task _handleLoginDataRequestAsync(ClientLoginDataRequest req)
        {
            var account = await _accountManager.GetAccountAsync(req.AccountID, false, false);

            if (account == null || (!account.IsOnline && !account.IsLoginPending))
            {
                // If the account isn't loaded yet, then it's definitely not logged in or logging in.
                // Refresh account
                account = await _accountManager.GetAccountAsync(req.AccountID, true, true);
                _connectionManager.AddPendingLogin(req.ClientAddress, account);
                account.IVKey = new IVKey(req.Key, req.IV);
                
                var msg = new NetworkMessageContainer();
                msg.MessageData = new MessageRedisLoginDataResponse(
                    ConnectionManager.ExternalIP.Address.GetAddressBytes(),
                    ConnectionManager.ExternalIP.Port,
                    account.Id,
                    LoginResult.Success
                );
                msg.MessageType = MessageTypes.Redis_LoginDataResponse;

                _redisServer.PublishObject(ChannelTypes.WebLogin, req.ProcessingWebServerId, msg);
            }
            else if (account.IsOnline) //Check if the account is already logged in
            {
                var msg = new NetworkMessageContainer();
                msg.MessageData = new MessageRedisLoginDataResponse(null, -1, req.AccountID, LoginResult.AlreadyLoggedOn);

                _redisServer.PublishObject(ChannelTypes.WebLogin, req.ProcessingWebServerId, msg);
            }
            else if (account.IsLoginPending) //Check if the login is already pending
            {
                var msg = new NetworkMessageContainer();
                msg.MessageData = new MessageRedisLoginDataResponse(null, -1, req.AccountID, LoginResult.AlreadyPending);

                _redisServer.PublishObject(ChannelTypes.WebLogin, req.ProcessingWebServerId, msg);
            }
        }

        private void _handleColonyDataPush(object sender, NetworkMessageContainer messageData)
        {
            var res = messageData?.MessageData as MessageColonyDataPush;

            _handleColonyDataPushAsync(res);
        }

        private async Task _handleColonyDataPushAsync(MessageColonyDataPush res)
        {
            var s = await _shipManager.GetShipAsync(res.ShipID);
            if (s?.CurrentAreaId == null)
            {
                return;
            }

            var a = _galaxyManager.GetArea(s.CurrentAreaId);
            if (a?.AreaType != AreaTypes.Colony)
            {
                return;
            }

            _handleDataPush(res, (Colony)a, s);
        }

        private void _handleAdminWarpPlayer(object sender, NetworkMessageContainer messageData)
        {
            var request = messageData?.MessageData as MessageAdminWarpPlayerRequest;

            var currentArea = _galaxyManager.GetArea(request.CurrentAreaId);

            // If this is true, our server doesn't own the client.
            if (currentArea?.AreaType == AreaTypes.Limbo)
            {
                return;
            }

            _handleAdminWarpPlayerAsync(request);
        }

        private async Task _handleAdminWarpPlayerAsync(MessageAdminWarpPlayerRequest request)
        {
            var ship = await _shipManager.GetShipAsync(request.ShipId);

            if (ship == null)
            {
                ConsoleManager.WriteLine("Ship was null in Admin Warp request", ConsoleMessageType.Warning);
            }

            await _warpManager.ChangeArea(request.NewAreaId, ship, false);

        }
    }
}

