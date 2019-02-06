using System;
using Freecon.Server.Core;
using Freecon.Core.Networking.Models;
using Freecon.Core.Networking;
using Freecon.Core.Utils;

namespace Freecon.Server.App
{
    /// <summary>
    /// Controls inbound requests and dispatching them to the router(s).
    /// </summary>
    public class DispatchManager
    {
        IClientRequestRouter _clientRequestRouter;
        ILoggerUtil _logger;
        IMessageSerializer _serializer;
        NetworkManager _networkManager;

        public DispatchManager(
            IClientRequestRouter clientRequestRouter,
            ILoggerUtil logger,
            IMessageSerializer serializer,
            NetworkManager networkManager)
        {
            _clientRequestRouter = clientRequestRouter;
            _logger = logger;
            _serializer = serializer;
            _networkManager = networkManager;
        }

        public void Start()
        {
            var stream = _networkManager.StartNodes();

            // Todo: Push this to a queue instead of publishing directly.
            stream.Subscribe(request => _clientRequestRouter.Publish(Deserialize(request)));
        }

        public void Publish(ICommMessage message)
        {
            _clientRequestRouter.Publish(message);
        }

        public ICommMessage Deserialize(RawClientRequest request)
        {
            var rawMessage = _serializer.DeserializeMessageContainer(request.RawRequest);

            return _serializer.DeserializePayload(rawMessage);
        }
    }
}
