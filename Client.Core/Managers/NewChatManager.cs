using System;
using System.Collections.Generic;
using Freecon.Client.Interfaces;
using Freecon.Client.Managers.Networking;
using Core.Interfaces;
using Freecon.Client.Mathematics;
using Freecon.Core.Networking.Models;
using Freecon.Core.Networking.Models.Messages;
using Freecon.Core.Utils;
using Freecon.Models.UI;
using Lidgren.Network;
using Newtonsoft.Json;

namespace Freecon.Client.Core.Managers
{
    public class ChatAddedEventArgs : EventArgs
    {
        public string ChatJson { get; protected set; }

        public ChatAddedEventArgs(string json)
        {
            ChatJson = json;
        }
    }

    public class NewChatManager : ISynchronousManager
    {
        protected ClientManager _clientManager;
        protected ChatboxModel _viewModel;

        public EventHandler<ChatAddedEventArgs> ChatAdded;

        public NewChatManager(ClientManager clientManager)
        {
            _clientManager = clientManager;
            _viewModel = new ChatboxModel();
        }

        public void AddChat(string chatJson, string metaJson)
        {
            var chat = JsonConvert.DeserializeObject<ClientChatLine>(chatJson);

            if (metaJson != null)
            {
                chat.Meta = JsonConvert.DeserializeObject<Dictionary<string, string>>(metaJson);
            }

            _viewModel.AddChat(chat);

            ChatAdded?.Invoke(this, new ChatAddedEventArgs(JsonConvert.SerializeObject(chat, Formatting.None)));
        }

        public void SendChatToServer(string chat)
        {
            var msg = _clientManager.Client.CreateMessage();

            var container = new NetworkMessageContainer();

            var data = new MessageChatMessage();
            data.ChatMessageData = new ChatMessageData(chat);
            container.MessageType = MessageTypes.ChatMessage;
            container.MessageData = data;

            container.WriteToLidgrenMessage_ToServer(MessageTypes.ChatMessage, msg);

            _clientManager.Client.SendMessage(msg, _clientManager.CurrentSlaveConnection, NetDeliveryMethod.ReliableOrdered);
        }

        public void AppendChatRaw(ClientChatLine chat)
        {
            _viewModel.AddChat(chat);
        }

        public string GetChatsJson()
        {
            return LowercaseContractResolver.SerializeObject(_viewModel.Chats);
        }

        public void Draw(Camera2D camera)
        {
        }

        public void Update(IGameTimeService gameTime)
        {

        }
    }
}
