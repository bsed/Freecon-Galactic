using Server.Managers;
using SRServer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Freecon.Core.Networking;
using Server.Managers.Factories;
using Core.Models.Enums;

namespace Freecon.Server.GameStates
{
    public abstract class ServerGameState<GameStateModel>
        where GameStateModel:ServerGameStateModel<GameStateStats>
    {
        protected GameStateModel _model;
                
        public abstract void Update();

        public abstract GameStateData GetStateForClient();

        public abstract void KillShip();

        protected ShipFactory _shipFactory;

        public ServerGameState(ShipFactory shipFactory)
        {
            _shipFactory = shipFactory;
        }

        public virtual bool CanAddPlayer()
        {
            return _model.Status != ServerGameStateStatus.Running && _model.PlayerIds.Count < _model.Stats.MaxNumPlayers;
        }

        /// <summary>
        /// Returns true if successful
        /// </summary>
        /// <param name="playerId"></param>
        /// <returns></returns>
        public virtual bool AddPlayer(int playerId)
        {
            if (!CanAddPlayer())
                return false;

            if (_model.PlayerIds.Contains(playerId))
            {
                throw new InvalidOperationException(playerId + " already exists in ServerGameState with Id " + _model.Id);
            }
            _model.PlayerIds.Add(playerId);
            return true;
        }

        
        public bool RemovePlayer(int playerId)
        {
            if (!_model.PlayerIds.Contains(playerId))
            {
                throw new InvalidOperationException(playerId + " already exists in ServerGameState with Id " + _model.Id);
            }
            _model.PlayerIds.Remove(playerId);
            return true;
        }

        public abstract Task Initialize();

        protected abstract ShipCreationProperties GetDefaultShipProperties(float posX, float posY, int areaId);
    }
}
