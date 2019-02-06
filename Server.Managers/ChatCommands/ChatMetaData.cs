using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Server.Models;

namespace Server.Managers.ChatCommands
{
    public class ChatMetaData
    {
        private Func<ChatMetaData, Task<List<OutboundChatMessage>>> _asyncCommand;
        private Func<ChatMetaData, List<OutboundChatMessage>> _command;

        public IArea Area { get; protected set; } 

        public Player Player { get; protected set; }

        public string UserInput { get; protected set; }

        public string CommandString { get; protected set; }

        public string Arguments { get; protected set; }

        public ChatMetaData(
            Player player,
            string userInput,
            string commandString,
            Func<ChatMetaData, List<OutboundChatMessage>> command,
            string arguments)
        {
            Area = player.GetArea();
            Player = player;
            UserInput = userInput;
            CommandString = commandString;
            _command = command;
            Arguments = arguments;
        }

        public ChatMetaData(
            Player player,
            string userInput,
            string commandString,
            Func<ChatMetaData, Task<List<OutboundChatMessage>>> asyncCommand,
            string arguments)
        {
            Area = player.GetArea();
            Player = player;
            UserInput = userInput;
            CommandString = commandString;
            _asyncCommand = asyncCommand;
            Arguments = arguments;
        }

        public async Task<List<OutboundChatMessage>> GetChatMessages()
        {
            if (_command != null)
            {
                return _command(this);
            }

            return await _asyncCommand(this);
        }
    }
}
