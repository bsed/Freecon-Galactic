using System.Collections.Generic;

namespace Freecon.Models.UI
{
    public class ChatboxModel
    {
        public List<ClientChatLine> Chats;

        public List<string> LastUserInputs; 

        public ChatboxModel()
        {
            Chats = new List<ClientChatLine>();
            LastUserInputs = new List<string>();
        }

        public void AddChat(ClientChatLine chatline)
        {
            Chats.Add(chatline);
        }

        public void AddUserChat(ClientChatLine chatline, string userInput)
        {
            AddChat(chatline);
            LastUserInputs.Add(userInput);
        }
    }
}
