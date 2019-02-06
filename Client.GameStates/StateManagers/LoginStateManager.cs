using Freecon.Client.Config;
using Core.Models.Enums;
using Freecon.Client.Core.Interfaces;
using Freecon.Client.Core.States;
using Freecon.Core.Networking.Models;
using Freecon.Core.Utils;
using Core.Interfaces;
using Newtonsoft.Json;
using Freecon.Client.Managers;
using Freecon.Client.Managers.Networking;
using System;
using System.Threading.Tasks;
using Freecon.Client.Core.States.Components;
using Freecon.Client.Managers.GUI;
using Freecon.Client.Mathematics;
using Freecon.Client.View;
using Freecon.Client.View.CefSharp;
using Freecon.Client.ViewModel;
using Freecon.Models.TypeEnums;
using Core.Web.Schemas;

namespace Freecon.Client.GameStates.StateManagers
{
    public class LoginStateManager : NetworkedGameState, IAsynchronousUpdate, IWebGameState<PortViewModel, PortWebView>
    {
        ClientManager _clientManager;
        MessageService_ToServer _messageService;

        object LOGINLOCK = new object();


        bool _awaitingResponse = false;
        double _attemptTime;//Time of login attempted, used for timeout
        float _timeout = 4000;//Time before retrying login

        bool _isLoggedIn = false;
        bool _loggingIn = false;

        LoginConfig _loginConfig;

        string Username = null;
        string Password = null;
        public bool ReadyToLogin { get; protected set; }

        /// <summary>
        /// Display this on login screen for user?
        /// </summary>
        public string LoginDisplayMessage = "";

        //TODO: Move this
        public byte[] EncryptionKey { get; protected set; }
        public byte[] EncryptionIV { get; protected set; }

        public PortWebView WebView { get; }
        public IHasGameWebView RawGameWebView => WebView;

        public LoginStateManager(
            LoginConfig loginConfig,
            ClientManager clientManager,
            GlobalGameUISingleton globalGameUiSingleton,
            MessageService_ToServer messageService,
            LidgrenNetworkingService networkingService)
            : base(null, null, networkingService, messageService, GameStateType.Login)
        {
            _clientManager = clientManager;

            // Todo: Unfuck this
            WebView = new PortWebView(globalGameUiSingleton, null);

            _loginConfig = loginConfig;

            //_gameStateManager = gameStateManager;

            networkingService.RegisterMessageHandler(this, _loginStateManager_MessageReceived);

            this._asynchronousUpdateList.Add(this);
        }

        public async Task Update(IGameTimeService gametime)
        {
            if (!_isLoggedIn && !_loggingIn && Status == GameStateStatus.Active)
            {
                await Login(gametime);
            }
        }

        public void LoginComplete()
        {
            _awaitingResponse = false;
            _isLoggedIn = true;

        }

        /// <summary>
        /// Returns true when succesful, false otherwise
        /// </summary>
        /// <param name="gametime"></param>
        /// <returns></returns>
        private async Task<LoginResult> Login(IGameTimeService gametime, int maxNumTries = 5)
        {
            
            int numTries = 0;

            _loggingIn = true;
#if DEVELOPMENT
            if (Debugging.Autologin)
            {
                await Task.Delay(8000);

                Username = Debugging.AutoLoginName;
                Password = Debugging.AutoLoginPassword;
                ReadyToLogin = true;
                Console.WriteLine("Autologin attempt as " + Debugging.AutoLoginName);
            }
            else
            {
                Console.Write("ENTER USERNAME: ");
                Username = Console.ReadLine();

                Console.Write("ENTER PASSWORD: ");
                Password = Console.ReadLine();
                ReadyToLogin = true;
            }

#endif

            if (!ReadyToLogin)
                return LoginResult.ServerNotReady;

            while (numTries < maxNumTries)
            {
                LoginDisplayMessage = "Logging in...";


                string response = await _clientManager.ConnectToURI(_buildLoginRequest(_loginConfig.LoginIP, _loginConfig.LoginPort, Username, Password));

                if (response == null)
                {
                    //Something went wrong...figure out how to deal with this later, if necessary
                    LoginDisplayMessage = "Something went wrong...";
                    _isLoggedIn = false;
                    _loggingIn = false;
                    return LoginResult.UnknownFailure;
                }
                else if (response == "timeout")
                {
                    //Response timed out, try again                     
                    numTries++;

                }
                else
                {
                    LoginResponse lr = JsonConvert.DeserializeObject<LoginResponse>(response);
                    EncryptionKey = lr.Key;
                    EncryptionIV = lr.IV;
                    Console.WriteLine("Login Response, " + lr.Result);

                    if (lr.Result == LoginResult.Success)
                    {

                        string IPAddress = IPConverter.Convert(lr.ServerIP);
                        bool connected = false;


                        await Task.Run(() =>
                        {
                            //_clientManager.ConnectToMasterServer(_loginConfig.MasterServerIP, _loginConfig.MasterServerPort);
                            connected = _clientManager.ConnectToSlaveServer(IPAddress, lr.ServerPort, lr.HailMessage, Username, Password, 2000);
                        });

                        if (!connected)
                        {
                            _loggingIn = false;
                            ReadyToLogin = false;
                            LoginDisplayMessage = "Could not connect to server.";
                            Console.WriteLine("Could not connect to slave. Connection refused? Ensure that networking is being updated.");
                        }

                        _loggingIn = false;
                        _isLoggedIn = true;
                        ReadyToLogin = false;
                        return LoginResult.Success;
                    }
                    else
                    {
                        if (lr.Result == LoginResult.InvalidUsernameOrPassword)
                        {
                            LoginDisplayMessage = "Invalid username or password.";
                        }
                        else if (lr.Result == LoginResult.AlreadyLoggedOn)
                        {
                            LoginDisplayMessage = "User is already logged on!";
                        }
                        else if (lr.Result == LoginResult.AlreadyPending)
                        {
                            //Ideally the user won't ever see this case...
                            LoginDisplayMessage = "User is already attempting to log on!";
                        }

                        _loggingIn = false;
                        ReadyToLogin = false;
                        return lr.Result;
                    }
                }
            }


            //All requests timed out, reset state
            _loggingIn = false;
            ReadyToLogin = false;
            LoginDisplayMessage = "Request timeout. Check your internet connection.";
            return LoginResult.MaxTimeoutsExceeded;


            
        }

        void _loginStateManager_MessageReceived(object sender, NetworkMessageContainer e)
        {

            switch (e.MessageType)
            {               
                case MessageTypes.ClientLoginFailed:
                                     

                    break;

            }
        }

        /// <summary>
        /// This will probably be used by the GUI, sets the login credentials and sets ReadyToLogin to true
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public void SetCredentialsLoginReady(string username, string password)
        {
            Username = username;
            Password = password;
            ReadyToLogin = true;

        }

        string _buildLoginRequest(string ip, int port, string username, string password)
        {
            return "http://" + ip + ":" + port + "/login/" + username + "_" + password;
        }

        public void Draw(Camera2D camera)
        {
        }

        public override void Activate(IGameState previous)
        {
            base.Activate(previous);
            _loggingIn = false;
            _isLoggedIn = false;
        }

        ~LoginStateManager()
        {
            _networkingService.DeregisterMessageHandler(this, _loginStateManager_MessageReceived);
        }
    }

}
