using System;
using System.Collections.Concurrent;
using System.Net;
using System.Runtime.ConstrainedExecution;
using System.Threading;
using System.Threading.Tasks;
using Core.Cryptography;
using Core.Models.Enums;
using Core.Web.Modules;
using Core.Web.Schemas;
using Freecon.Core.Networking.Models;
using Freecon.Core.Networking.ServerToServer;
using Freecon.Core.Utils;
using Freecon.Models.TypeEnums;
using Freecon.Server.Core.Services;
using Nancy;
using Nancy.Cookies;
using Newtonsoft.Json;
using RedisWrapper;
using Server.Database;
using Server.Models;

namespace Core.Web.NancyModules
{
    public class LoginHandler : BaseHandler
    {
        private readonly IDatabaseManager _databaseManager;
        private readonly CryptographyManager _cryptographyManager;
        private readonly RedisServer _redisServer;
        private readonly ILoggerUtil _logger;
        private readonly SessionService _sessionService;

        protected readonly int _instanceId;//Ensures unique redis subscriptions among instances

        private readonly float _redisResponseTimeout = 2000; //ms


        private static readonly ConcurrentDictionary<int, MessageRedisLoginDataResponse> _IDToConnectionAddress =
            new ConcurrentDictionary<int, MessageRedisLoginDataResponse>();

        public LoginHandler(
            IDatabaseManager dbm, 
            CryptographyManager cpm,
            RedisServer rs,
            ILoggerUtil logger,
            SessionService sessionService)
            : base(RouteConfig.Login)
        {
            // Todo: Refactor this into POST request to mitigate CSRF.
            // http://www.jhovgaard.com/nancy-csrf/
            Get[RouteConfig.Login_Request, true] = _handleLogin;

            _databaseManager = dbm;
            _cryptographyManager = cpm;
            _redisServer = rs;
            _logger = logger;
            _sessionService = sessionService;
            
            //This might add a bit of latency to login requests, but I doubt it will be a problem. Adding this comment in case we need to debug it in the future.
            _instanceId = rs.GenerateUniqueId().Result;
            
            _redisServer.Subscribe(ChannelTypes.WebLogin, _instanceId, _handleRedisLoginDataResponse);
            
            
        }

        ~LoginHandler()
        {
            _redisServer.UnSubscribe(ChannelTypes.WebLogin, _instanceId, _handleRedisLoginDataResponse);
        }

        private async Task<dynamic> _handleLogin(dynamic parameters, CancellationToken cancellationToken)
        {
            string username = parameters["username"];
            string password = parameters["password"];
            bool apiOnly = Request.Query["api-only"];

            _logger.Log("Handling login attempt from " + username, LogLevel.Verbose);

            var am = await _databaseManager.GetAccountAsync(username);

            var lr = new LoginResponse();

            if (am == null)
            {
                _logger.Log(username + " login failed: account not found.", LogLevel.Verbose);

                lr.Result = LoginResult.InvalidUsernameOrPassword;

                return ReturnJsonResponse(lr);
            }
            if (am.Password != password)
            {
                _logger.Log(username + " login failed: incorrect password.", LogLevel.Verbose);

                lr.Result = LoginResult.InvalidUsernameOrPassword;

                return ReturnJsonResponse(lr);
            }

            //else if(_IDToConnectionAddress.ContainsKey(am.Id))
            //{
            //    //Ignore
            //}

            var ivkey = _cryptographyManager.GenerateIVKey();
            lr.IV = ivkey.IV;
            lr.Key = ivkey.Key;

            // Grab player so that the UI knows what state the load initially.
            var player = await _databaseManager.GetPlayerAsync(username);

            var currentArea = await _databaseManager.GetAreaAsync(player.CurrentAreaID.Value);

            var areaType = player.CurrentAreaID.HasValue
                ? currentArea.AreaType
                : AreaTypes.Limbo;

            lr.CurrentAreaType = ConvertAreaToInterfaceState(areaType);

            var clientAddress = IPAddress.Parse(Request.UserHostAddress);

            var currentSession = await _sessionService.FindPlayerSession(am.PlayerID);
            
            // Client already logged in
            if (currentSession != null)
            {
                _logger.Log(username + " duplicate login, continuing.", LogLevel.Verbose);
                
                lr.ApiToken = currentSession.ApiToken;
            }
            else
            {

                // Can throw an exception, but we probably want to fail over if it does.
                var session = await _sessionService.CreatePlayerSession(am, clientAddress);

                lr.ApiToken = session.ApiToken;
            }

            //Wait for redis response
            var response = await PublishToSlave(am, lr, clientAddress);

            if (response == null)
            {
                var msg =
                    "Error: no ServerConnectionAddress response recieved from redis. Possible causes: no slaves running, no slaves handling given account, account doesn't exist.";
                Console.WriteLine(msg);
                lr.Result = LoginResult.ServerNotReady;

                //throw new Exception(msg);
            }
            else if ((response.Result == LoginResult.AlreadyLoggedOn) || (response.Result == LoginResult.AlreadyPending))
            {
                Console.WriteLine(username + ": " + response.Result);
                lr.Result = response.Result;
            }
            else
            {
                lr.ServerIP = response.ServerIP;
                lr.ServerPort = response.ServerPort;
                lr.Result = LoginResult.Success;
#if DEBUG
                Console.WriteLine(username + " has logged in succesfully. Notifying slave...");
#endif
            }

            return ReturnJsonResponse(lr);
        }

        public string ConvertAreaToInterfaceState(AreaTypes area)
        {
            switch (area)
            {
                case AreaTypes.Colony:
                    return AreaTypes.Colony.ToString();
                case AreaTypes.Planet:
                    return AreaTypes.Planet.ToString();
                case AreaTypes.Port:
                    return AreaTypes.Port.ToString();
                default:
                    return "Space";
            }
        }

        private async Task<MessageRedisLoginDataResponse> PublishToSlave(AccountModel am, LoginResponse lr, IPAddress clientAddress)
        {
            // Publish login to redis to notify appropriate slave
            try
            {
                var msg = new NetworkMessageContainer();
                msg.MessageData = new ClientLoginDataRequest(
                    _instanceId,
                    am.Id,
                    am.LastSystemID,
                    lr.IV,
                    lr.Key,
                    am.Username,
                    am.Password,
                    clientAddress.GetAddressBytes()
                );
                msg.MessageType = MessageTypes.Redis_LoginDataRequest;
                _redisServer.PublishObject(MessageTypes.Redis_LoginDataRequest, msg);
            }
            catch (Exception e)
            {
                _logger.Log(e.Message, LogLevel.Error);
            }

            return await _getResponse(am.Id);
        }

        private void _handleRedisLoginDataResponse(object sender, NetworkMessageContainer messageData)
        {
            var res = messageData.MessageData as MessageRedisLoginDataResponse;
            if (_IDToConnectionAddress.ContainsKey(res.AccountID))
                Console.WriteLine("Error: Redis received multiple ClientLoginDataReponse objects");

            //TODO:Figure out why this happens. If a player logs in more than once without restarting the server, this error state occurs.
            else
                _IDToConnectionAddress.TryAdd(res.AccountID, res);
        }

        private async Task<MessageRedisLoginDataResponse> _getResponse(int accountID)
        {
            var waitStartTime = TimeKeeper.MsSinceInitialization;
            MessageRedisLoginDataResponse response = null;

            await Task.Run(async () =>
            {
                while (!_IDToConnectionAddress.TryRemove(accountID, out response))
                {
                    if (TimeKeeper.MsSinceInitialization - waitStartTime > _redisResponseTimeout)
                        return;

                    await Task.Delay(1);
                }

                await Task.Delay(10);
            });

            return response;
        }
    }
}
