using Freecon.Client.Config;
using Core.Networking.Interfaces;
using Freecon.Core.Networking.Interfaces;
using Freecon.Core.Networking.Models;
using RedisWrapper;
using System;
using System.Collections.Generic;
using Core.Models.Enums;
using Core.Networking;
using Freecon.Core.Networking.Objects;
using Server.Managers;

namespace Freecon.Simulator
{
    /// <summary>
    /// This service is designed to operate synchronously within an update loop.
    /// </summary>
    public class RedisNetworkingService : INetworkingService
    {
        SimulatorConfig _config;
        public RedisServer RedisServer { get { return _redisServer; } }
        RedisServer _redisServer;

        
        Dictionary<int, EventHandler<NetworkMessageContainer>> _areaIDToHandlerMethod;
        private Dictionary<int, INetworkMessageHandler> _areaIDToHandlerObject;

        /// <summary>
        /// Key is AreaID
        /// </summary>
        Dictionary<int, List<SimulatorBoundMessage>> _pendingMessages;

        public RedisNetworkingService(SimulatorConfig config, RedisServer redisServer)
        {
            _config = config;
            _redisServer = redisServer;
            _areaIDToHandlerMethod = new Dictionary<int, EventHandler<NetworkMessageContainer>>();
            _areaIDToHandlerObject = new Dictionary<int, INetworkMessageHandler>();
            _pendingMessages = new Dictionary<int, List<SimulatorBoundMessage>>();
        }

        public void RegisterMessageHandler(INetworkMessageHandler handlerObject, EventHandler<NetworkMessageContainer> handlerMethod)
        {
            //_redisServer.Subscribe(ChannelTypes.SimulatorData, handlerObject.MessageHandlerID.Value, handlerMethod);
            if (!_areaIDToHandlerMethod.ContainsKey(handlerObject.MessageHandlerID.Value))
            {
                _areaIDToHandlerMethod.Add(handlerObject.MessageHandlerID.Value, handlerMethod);
                _areaIDToHandlerObject.Add(handlerObject.MessageHandlerID.Value, handlerObject);
                _redisServer.Subscribe(ChannelTypes.ServerToSimulator_Data, handlerObject.MessageHandlerID.Value, AddPendingMessage);//Putting this here should ensure unique subscriptions. If the same handler is subscribed twice, it will be called twice
                _pendingMessages.Add(handlerObject.MessageHandlerID, new List<SimulatorBoundMessage>());
            }
            else
            {
                _areaIDToHandlerMethod[handlerObject.MessageHandlerID.Value] += handlerMethod;
            }
        }

        public void DeregisterMessageHandler(INetworkMessageHandler handlerObject, EventHandler<NetworkMessageContainer> handlerMethod)
        {
            if (_areaIDToHandlerMethod.ContainsKey(handlerObject.MessageHandlerID.Value))
            {
                _areaIDToHandlerMethod[handlerObject.MessageHandlerID.Value] -= handlerMethod;

                if (_areaIDToHandlerMethod[handlerObject.MessageHandlerID.Value] == null)
                {
                    _areaIDToHandlerMethod.Remove(handlerObject.MessageHandlerID);
                    _areaIDToHandlerObject.Remove(handlerObject.MessageHandlerID);
                    _pendingMessages.Remove(handlerObject.MessageHandlerID);
                }
            }

          

            if(!_areaIDToHandlerMethod.ContainsKey(handlerObject.MessageHandlerID.Value))
            {
                _redisServer.UnSubscribe(ChannelTypes.ServerToSimulator_Data, handlerObject.MessageHandlerID.Value, AddPendingMessage);//If there are no more handlers registered for this areaID, unsubscribe from redis. Not sure if this will throw if there is no current subscription, hopefully it won't
            }


        }

        public void DeregisterAllHandlers(INetworkMessageHandler handlerObject)
        {
            _redisServer.UnSubscribe(ChannelTypes.ServerToSimulator_Data, handlerObject.MessageHandlerID, AddPendingMessage);
            _areaIDToHandlerMethod.Remove(handlerObject.MessageHandlerID);
            _areaIDToHandlerObject.Remove(handlerObject.MessageHandlerID);
            _pendingMessages.Remove(handlerObject.MessageHandlerID);
        }
          
        /// <summary>
        /// Synchronously processes all pending messages for the corresponding areaID
        /// </summary>
        /// <param name="areaID"></param>
        public void FlushMessages(MessageHandlerID messageHandlerID)
        {
            for (int i = 0; i < _pendingMessages[messageHandlerID.Value].Count; i++)
            {
                var message = _pendingMessages[messageHandlerID.Value][i];
               
                _areaIDToHandlerMethod[(int)message.TargetAreaId](this, message);
            }

            _pendingMessages[messageHandlerID.Value].Clear();
        }

        void AddPendingMessage(object sender, NetworkMessageContainer message)
        {
            _pendingMessages[((SimulatorBoundMessage)message).TargetAreaId].Add((SimulatorBoundMessage)message);
        }

    }
    
}
