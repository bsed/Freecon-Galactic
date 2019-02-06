using Core.Models.Enums;
using Freecon.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Freecon.Server.GameStates
{
    public class ServerGameStateModel<TGameStateStats> : IDBObject, IServerGameStateModel
        where TGameStateStats: GameStateStats
    {
        public int Id { get; set; }
        
        public int EntryAreaId { get; set; }//Area where ships entering the GameState are sent

        public ModelTypes ModelType { get { return ModelTypes.ServerGameStateModel; } }

        public ServerGameStateTypes GameStateType { get; protected set; }
                
        public string GameStateName { get; set; }

        public HashSet<int> SystemIDs { get; set; }

        public GameStateStats Stats { get; set; }

        public HashSet<int> PlayerIds { get; set; }

        public ServerGameStateStatus Status { get; set; }
    }
}
