using Freecon.Core.Networking.Models;
using System.Collections.Generic;

namespace Freecon.Server.Core
{
    public class ClientResponse
    {
        public ICommMessage Message { get; private set; }

        public IEnumerable<IClientNode> Clients { get; private set; }

        public ClientResponse(ICommMessage message, IEnumerable<IClientNode> clients)
        {
            Message = message;
            Clients = clients;
        }

        public ClientResponse(ICommMessage message, IClientNode client)
        {
            Message = message;
            Clients = new List<IClientNode>() { client };
        }
    }
}
