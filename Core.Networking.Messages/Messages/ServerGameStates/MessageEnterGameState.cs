using Freecon.Core.Networking.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Freecon.Core.Networking
{
    public class MessageEnterGameState : MessagePackSerializableObject
    {
        public string GameStateName { get { return _gameStateName; } set { _gameStateNameSet = true; _gameStateName = value; } }
        string _gameStateName;
        bool _gameStateNameSet;

        public string GameStateId { get { return _gameStateId; } set { _gameStateIdSet = true; _gameStateId = value; } }
        string _gameStateId;
        bool _gameStateIdSet;

        public string GameStateData { get { return _gameStateData; } set { _gameStateDataSet = true; _gameStateData = value; } }
        string _gameStateData;
        bool _gameStateDataSet;

       public string EntryMessage { get; set; }

        public override byte[] Serialize()
        {
            if (!_gameStateNameSet)
                throw new RequiredParameterNotInitialized("GameStateName", this);
            if (!_gameStateIdSet)
                throw new RequiredParameterNotInitialized("GameStateId", this);
            if (!_gameStateDataSet)
                throw new RequiredParameterNotInitialized("GameStateData", this);

            return base.Serialize();
        }

    }

    public class GameStateData:MessagePackSerializableObject
    {


    }


}
