using System.Collections.Generic;

namespace Server.Managers.ChatCommands
{
    public abstract class BaseChatCommand
    {
        public List<string> CommandSignatures { get; protected set; }

        public BaseChatCommand()
        {
            CommandSignatures = new List<string>();
        }
    }
}