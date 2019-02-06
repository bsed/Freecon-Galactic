namespace Freecon.Server.Core
{
    public class ClientRequest<TMessage>
    {
        public TMessage Message { get; private set; }

        public IClientNode Client { get; private set; }

        public ClientRequest(TMessage message, IClientNode client)
        {
            Message = message;
            Client = client;
        }
    }

    public class RawClientRequest
    {
        public byte[] RawRequest { get; private set; }

        public IClientNode Client { get; private set; }

        public RawClientRequest(byte[] rawRequest, IClientNode client)
        {
            RawRequest = rawRequest;
            Client = client;
        }
    }
}
