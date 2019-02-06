namespace Freecon.Core.Networking.Models.Messages
{
    public class MessageColonizeRequestDenial:MessagePackSerializableObject
    {
        public string DenialMessage { get { return _denialMessage; } set { _denialMessageSet = true; _denialMessage = value; } }
        string _denialMessage;
        bool _denialMessageSet;

        public override byte[] Serialize()
        {
            if (!_denialMessageSet)
                throw new RequiredParameterNotInitialized("DenialMessage", this);


            return base.Serialize();
        }
    }
}
