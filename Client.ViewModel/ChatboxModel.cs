using System.Collections.Generic;
using Core.Interfaces;
using Freecon.Client.Core.Interfaces;
using Freecon.Models.ChatCommands;

namespace Freecon.Client.ViewModel
{
    public class ChatboxModel : IViewModel
    {
        public List<ChatlineObject> Chats;

        public List<string> LastUserInputs; 

        public ChatboxModel()
        {
            Chats = new List<ChatlineObject>();
            LastUserInputs = new List<string>();
        }

        public void AddChat(ChatlineObject chatline)
        {
            Chats.Add(chatline);
        }

        public void AddUserChat(ChatlineObject chatline, string userInput)
        {
            AddChat(chatline);
            LastUserInputs.Add(userInput);
        }

        public void Update(IGameTimeService gameTime)
        {

        }
    }
}
