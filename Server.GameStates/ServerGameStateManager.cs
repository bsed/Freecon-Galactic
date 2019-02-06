using Core.Models.Enums;
using Server.Managers;
using Server.Managers.Economy;
using SRServer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Freecon.Server.GameStates
{
    public class ServerGameStateManager:IServerGameStateManager
    {
        public Dictionary<int, ServerGameState<ServerGameStateModel<GameStateStats>>> _serverGameStates;

        IPlayerLocator _playerLocator;
        IAreaLocator _areaLocator;
        ITeamLocator _teamLocator;
        ITradeTerminator _tradeTerminator;
        MessageManager _messageManager;
        WarpManager _warpManager;
        ChatManager _chatManager;


        public ServerGameStateManager(IPlayerLocator pl, IAreaLocator al, ITeamLocator tl, MessageManager mm, WarpManager wm, ChatManager cm, ITradeTerminator tt)
        {
            _playerLocator = pl;
            _areaLocator = al;
            _teamLocator = tl;
            _messageManager = mm;
            _warpManager = wm;
            _chatManager = cm;
            _tradeTerminator = tt;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameStateType"></param>
        /// <returns>Id of the created ServerGameStateObject</returns>
        public int CreateServerGameState<TInitParams>(TInitParams params)
            where TInitParams
        {
            
        }

        public void MovePlayerToGameState(int gameStateId, int playerId)
        {
            if (_serverGameStates.ContainsKey(gameStateId))
            {
                var gs = _serverGameStates;
                
                }


        }

        public void _handleGameStateJoinRequest()
        {
            //Recieved request to allow a player on another server to join
            var shipProps = _shipFactory.CreateShip(GetDefaultShipProperties(5, 5, _model.SpawnSystemId), _playerLocator.GetPlayerAsync(j)
        }
    }
}
