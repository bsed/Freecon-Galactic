using System;
using Core.Database;
using Freecon.Core.Utils;
using Freecon.Server.Configs;
using Freecon.Server.Core.Services;
using Freecon.Server.Core;
using Freecon.Server.Core.Subscriptions;

/**
 * Responsibilities:
 * Tracks the state of sessions from clients.
 * A session is defined as:
 * -> A socket connection to a client.
 * Sessions have meta data attached to them.
 */ 

namespace Freecon.Server.App
{
    public class SessionManager
    {
        private SessionManagerConfig _sessionManagerConfig;
        private ILoggerUtil _logger;
        private DatabaseHelpers _databaseHelpers;
        private IMessageSendingService _messageSendService;
        private IMessageStreamService _messageStreamService;

        public SessionManager(
            SessionManagerConfig sessionManagerConfig,
            ILoggerUtil logger,
            DatabaseHelpers databaseHelpers,
            IMessageSendingService messageSendService,
            IMessageStreamService messageStreamService
            )
        {
            _logger = logger;

            _sessionManagerConfig = sessionManagerConfig;

            _databaseHelpers = databaseHelpers;
            _messageSendService = messageSendService;
            _messageStreamService = messageStreamService;

            Subscribe();
        }

        public void Subscribe()
        {
            _messageStreamService.RawMessageStream.Subscribe(
                StreamUpdate,
                (e) => { }, // OnError
                () => { }   // OnCompleted
            );

            //_messageStreamService.ClientStatus.Subscribe(
            //    ClientStatus,
            //    (e) => { },
            //    () => { }
            //);
        }

        public void AddValidator()
        {

        }

        private void StreamUpdate(RawClientRequest clientRequest)
        {
            //if (clientRequest.Message.PayloadType == MessageTypes.ClientLoginRequest)
            //{
            //    //var login = _databaseHelpers.PerformLoginCheck(clientRequest.;

            //    //Task.WhenAny(login, Task.Delay(_sessionManagerConfig.LoginTimeout));
            //}
        }

        private void ClientStatus(ClientStatusChanged statusChanged)
        {
        }
    }
}
