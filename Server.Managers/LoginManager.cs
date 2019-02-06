using System;
using Lidgren.Network;
using Freecon.Models.TypeEnums;
using Freecon.Core.Networking.Models;
using Server.Models;
using System.Threading.Tasks;
using Core.Models.Enums;
using Freecon.Core.Networking.Models.Messages;
using Freecon.Models.ChatCommands;
using Server.Interfaces;
using Server.Managers.ChatCommands;
using SRServer.Services;
using Freecon.Core.Networking.Models.Objects;
using Freecon.Core.Networking.Objects;
using RedisWrapper;

namespace Server.Managers
{
    public partial class LoginManager
    {

        AccountManager _accountManager;
        PlayerManager _playerManager;
        ConnectionManager _connectionManager;
        RedisServer _redisServer;

        string message = "";

        public LoginManager(AccountManager accountManager, PlayerManager pm, ConnectionManager cm, RedisServer redisServer)
        {
            _accountManager = accountManager;
            _playerManager = pm;
            _connectionManager = cm;
            _redisServer = redisServer;
        }

        /// <summary>
        /// Handles login of a client. Returns true if login succesful
        /// </summary>
        /// <param name="receiveMessage">Incoming Message</param>
        /// <param name="accountManager">Account Manager</param>
        /// <param name="sendMessage">Blank message to write to.</param>
        /// <param name="messageConnection">Peer that is logging in, used to send success/fail.</param>
        public async Task HandleLogin(NetConnection messageConnection, Account a, ChatManager chatManager, SimulatorManager simulationManager, LocatorService ls)
        {
            try
            {
                HumanPlayer tempPlayer = (HumanPlayer)await _playerManager.GetObjectAsync(a.PlayerID, true, ls, true);
                _redisServer.SetHashValue(RedisDBKeyTypes.PlayerIDToCurrentAreaID, tempPlayer.Id, tempPlayer.CurrentAreaID);
                tempPlayer.IsOnline = true;
                tempPlayer.MessageService = new LidgrenOutgoingMessageService(messageConnection, _connectionManager);
                var currentArea = tempPlayer.GetArea();

                a.connection = messageConnection;

                _playerManager.connectionToPlayer.TryAdd(messageConnection, tempPlayer);

                tempPlayer.GetArea().AddPlayer(tempPlayer, false);

                SendLoginList(currentArea, tempPlayer, chatManager);

                //currentArea.MovePlayerHere(tempPlayer, false);
                currentArea.SendEntryData(tempPlayer, false, tempPlayer.GetActiveShip());
                
                tempPlayer.lastHeartbeat = DateTime.Now.Ticks / 10000L;

                
                var SuccessMessage = string.Format("Player {0} has logged in successfully, current area: {1}", new object[2]{a.Username, tempPlayer.CurrentAreaID});
                ConsoleManager.WriteLine(SuccessMessage);

                tempPlayer.HackCount = -1;

                simulationManager.SendMessageToSimulator(new SimulatorBoundMessage(new MessageEmptyMessage{Data = currentArea.NumOnlineHumanPlayers}, MessageTypes.Redis_NumOnlinePlayersChanged, currentArea.Id));
            }
            catch(Exception e)
            {                
                ConsoleManager.WriteLine(e.Message, ConsoleMessageType.Error);
                ConsoleManager.WriteLine(e.StackTrace, ConsoleMessageType.Notification);
            }
           
        }

        public string GetGalacticNews(Player player)
        {
            string text = "";
            char LineSplit = '}';

            text += "Welcome, " + player.Username + " to Freecon Galactic. (Alpha)" + LineSplit;
            text += "You are currently in " + player.GetArea().AreaName + "." + LineSplit;
            text += "You currently pilot a";
            switch (player.GetActiveShip().ShipStats.ShipType)
            {
                case ShipTypes.Barge:
                    text += " Penis";
                    break;
                case ShipTypes.FrogShip:
                    text += " Rainbow Frog";
                    break;
                case ShipTypes.Penguin:
                    text += " Jeyth";
                    break;
                case ShipTypes.BattleCruiser:
                    text += " Kickass Battlecruiser!";
                    break;
                default:
                case ShipTypes.XBoxController:
                    text += "n Awesome";
                    break;
            }
            text += " class ship." + LineSplit;
            text += "Server time is " + DateTime.Now.ToString("f") + LineSplit;
            text += "Good day to you!";
            return text;
        }


        
        /// <summary>
        /// Sends playerID
        /// Sends info for all players in system at time of login
        ///  
        /// </summary>
        /// <param name="?"></param>
        private void SendLoginList(IArea area, HumanPlayer player, ChatManager chatManager)
        {
            IShip ship = player.GetActiveShip();
            MessageClientLogin data = new MessageClientLogin();
            data.Ship = ship.GetNetworkData(true, true, true, true);
            data.CurrentCash = player.CashOnHand;
            data.LoginMessage = GetGalacticNews(player);
            data.AreaName = area.AreaName;
            data.PlayerInfo = new PlayerInfo(player.Id, player.ActiveShipId);


            player.SendMessage(new NetworkMessageContainer(data, MessageTypes.ClientLoginSuccess));
               
            var outboundChat = new OutboundChatMessage(
                player,
                new ChatlineObject("Welcome to Freecon Galactic, " + player.Username + ".", ChatlineColor.White)
            );

            chatManager.SendChatToPlayer(outboundChat);
        }
       
    }
}
