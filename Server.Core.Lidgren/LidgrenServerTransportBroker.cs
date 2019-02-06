using Freecon.Core.Configs;
using Freecon.Server.Core;
using Freecon.Server.Core.Lidgren;
using Freecon.Server.Core.Subscriptions;
using Lidgren.Network;
using System;
using System.Reactive.Subjects;
using System.Threading;
using Freecon.Core.Utils;

namespace Freecon.Core.Networking.Lidgren
{
    public class LidgrenServerTransportBroker : IServerTransportBroker
    {
        private IServerTransportNode _transportNode;
        private NetPeerConfiguration _config;
        private NetServer _server;
        private ILoggerUtil _logger;
        private Subject<ServerStatusChanged> _serverStatusChanged;

        public LidgrenServerTransportBroker(INetworkConfig netConfig, ILoggerUtil logger)
        {
            _config = SetupConfiguration(netConfig);
            _server = new NetServer(_config);
            _logger = logger;
        }

        public IServerTransportNode Start(Func<IClientNode, bool> validateClientCallback)
        {
            // If no validation callback was specified, just return true.
            Func<IClientNode, bool> validateClient = validateClientCallback != null 
                                                        ? validateClientCallback
                                                        : validateClient = (c) => true;

            _server.Start();
            _serverStatusChanged = new Subject<ServerStatusChanged>();

            var rxStream = new Subject<RawClientRequest>();
            var clientStatusStream = new Subject<ClientStatusChanged>();
            
            _server.RegisterReceivedCallback(new SendOrPostCallback((obj) =>
            {
                NetIncomingMessage message;
                while ((message = _server.ReadMessage()) != null)
                {
                    switch (message.MessageType)
                    {
                        case NetIncomingMessageType.Data:
                            LidgrenClientNode client = null;

                            if (message.SenderConnection != null) {
                                client = new LidgrenClientNode(message.SenderConnection);
                            }

                            // Todo: Perform session validation here. Maybe?

                            rxStream.OnNext(new RawClientRequest(ReadMessage(message), client));

                            break;

                        case NetIncomingMessageType.VerboseDebugMessage:
                        case NetIncomingMessageType.DebugMessage:
                        case NetIncomingMessageType.WarningMessage:
                        case NetIncomingMessageType.ErrorMessage:
                            _logger.Log("Unhandled type: " + message.MessageType, LogLevel.Verbose);
                            break;

                        case NetIncomingMessageType.ConnectionApproval:
                            // Todo: Pass in node.
                            if (validateClient(null))
                            {
                                message.SenderConnection.Approve();
                            } 
                            else 
                            {
                                message.SenderConnection.Deny();
                            }

                            break;

                        default:
                            _logger.Log("Unhandled type: " + message.MessageType, LogLevel.Warning);
                            break;
                    }

                    _server.Recycle(message);
                }

            }), new SynchronizationContext());

            _transportNode = new ServerTransportNode(rxStream, clientStatusStream, _serverStatusChanged);

            return _transportNode;
        }

        public void Stop()
        {
            // Give our services a heads up, in case they need to clean up any state.
            _serverStatusChanged.OnNext(new ServerStatusChanged(ServerStatusChangeType.Stopping));
            _server.Shutdown("Good bye everybody!");
            _serverStatusChanged.OnNext(new ServerStatusChanged(ServerStatusChangeType.Stopped));
        }

        public NetPeerConfiguration SetupConfiguration(INetworkConfig netConfig)
        {
            var myConfig = new NetPeerConfiguration(netConfig.ServerName);

            myConfig.ConnectionTimeout = 5;
            myConfig.ReceiveBufferSize = 5000000;
            myConfig.SendBufferSize = 5000000;
            myConfig.Port = netConfig.Port;
            myConfig.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
            myConfig.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
            myConfig.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
            //myConfig.SimulatedLoss = 0.1f; // Fun to play around with.

            myConfig.AcceptIncomingConnections = true;
            //byte[] externalAddressBytes = GetExternalIPAddress().GetAddressBytes();
            //var externalIP = externalAddressBytes;

			return myConfig;
        }

        public byte[] ReadMessage(NetIncomingMessage message)
        {
            return message.ReadBytes(message.LengthBytes);
        }
    }
}
