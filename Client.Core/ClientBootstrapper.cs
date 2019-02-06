using Freecon.Core.Networking.Proto;

namespace Freecon.Client.Core
{
    public class ClientBootstrapper
    {
        public ClientBootstrapper()
        {
            ProtobufMappingSetup.Setup();
        }
    }
}
