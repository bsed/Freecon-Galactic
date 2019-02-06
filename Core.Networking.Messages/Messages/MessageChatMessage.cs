namespace Freecon.Core.Networking.Models.Messages
{
    public class MessageChatMessage:MessagePackSerializableObject
    {
    
        public ChatMessageData ChatMessageData { get { return _chatMessageData; } set { _chatMessageDataSet = true; _chatMessageData = value; } }
        ChatMessageData _chatMessageData;
        bool _chatMessageDataSet;

        public override byte[] Serialize()
        {
            //TODO:Consider replacing required parameter setter checks with custom attribute [RequiredParameterAttribte]? Might require caching of class metadata to avoid reflection for each write.

            if (!_chatMessageDataSet)
                throw new RequiredParameterNotInitialized("ChatMessageData", this);

            return base.Serialize();
        }
    }

    public class ChatMessageData
    {
        public string ChatJson { get; set; }

        public string MetaJson { get; set; }


        public ChatMessageData() { }

        public ChatMessageData(string json)
        {
            ChatJson = json;
        }

        public ChatMessageData(string json, string metaJson)
        {
            ChatJson = json;
            MetaJson = metaJson;
        }
    }
}
