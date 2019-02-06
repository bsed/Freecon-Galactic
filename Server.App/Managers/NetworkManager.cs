using Freecon.Core.Configs;
using Freecon.Core.Networking;
using Freecon.Core.Networking.Lidgren;
using Freecon.Server.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Freecon.Core.Utils;

namespace Freecon.Server.App
{
    public class NetworkManager
    {
        private ConfigManager _configManager;
        private ILoggerUtil _logger;
        private IMessageSerializer _messageSerializer;

        private List<ServerNode> _serverNodes;

        public IObservable<RawClientRequest> ConcatStream { get; private set; }

        public NetworkManager(
            IMessageSerializer messageSerializer,
            ConfigManager configManager,
            ILoggerUtil logger)
        {
            _messageSerializer = messageSerializer;
            _configManager = configManager;
            _logger = logger;

            _serverNodes = new List<ServerNode>();
        }

        public IObservable<RawClientRequest> StartNodes()
        {
            // Iterate through the config and launch each node.
            foreach (var config in _configManager.NetworkConfigs)
            {
                switch (config.Type)
                {
                    case NetworkConfigType.Lidgren:
                        _serverNodes.Add(SetupLidgrenServerNode(config));
                        break;

                    // Currently replicates Lidgren. As we add auth and other logic,
                    // This can be told to disable things like secure sessions, etc.
                    case NetworkConfigType.Test:
                        _serverNodes.Add(SetupLidgrenServerNode(config));
                        break;
                }
            }

            _serverNodes.ForEach(p => p.Start());

            // Combine the streams of messages into an agnostic, concatenated stream.
            ConcatStream = Observable.Concat(_serverNodes.Select(p => p.MessageStream));

            return ConcatStream;
        }

        private ServerNode SetupLidgrenServerNode(INetworkConfig config)
        {
            var broker = new LidgrenServerTransportBroker(config, _logger);

            return new ServerNode(broker, _messageSerializer, _logger);
        }
    }
}
