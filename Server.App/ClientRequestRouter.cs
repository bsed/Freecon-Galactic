using Freecon.Core.Networking.Models;
using Freecon.Core.Networking.Models.Objects;
using Freecon.Server.Core;
using Freecon.Server.Core.Reactive;
using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using Freecon.Core.Utils;

namespace Freecon.Server.App
{
    /// <summary>
    /// Routes client requests to subscribers.
    /// </summary>
    public class ClientRequestRouter : IClientRequestRouter
    {
        private IClientCommRouter _clientCommRouter;
        private ILoggerUtil _logger;

        private Dictionary<TodoMessageTypes, Action<ICommMessage>> _routes;

        public ClientRequestRouter(
            IClientCommRouter clientCommRouter,
            ILoggerUtil logger)
        {
            _clientCommRouter = clientCommRouter;
            _logger = logger;

            _routes = new Dictionary<TodoMessageTypes, Action<ICommMessage>>();
        }

        /// <summary>
        /// Takes in an unmapped ICommMessage and maps + routes it to subscribers.
        /// </summary>
        /// <param name="message"></param>
        public void Publish(ICommMessage message)
        {
            if (!_routes.ContainsKey(message.PayloadType))
            {
                _logger.Log("Unknown message type in router. " + message.PayloadType, LogLevel.Error);
                return;
            }

            _routes[message.PayloadType](message);
        }

        /// <summary>
        /// Registers message type enums to <typeparamref name="T"/> type.
        /// </summary>
        /// <typeparam name="T">Generic ICommMessage param to map enum to.</typeparam>
        /// <param name="type">MessageType enum value to map from when message is read.</param>
        public void RegisterType<T>(TodoMessageTypes type) where T : ICommMessage
        {
            if (_routes.ContainsKey(type))
            {
                throw new Exception("Already handled");
            }

            var subject = new Subject<ClientCommMessage<T>>();
            _clientCommRouter.RegisterPublisher<T>(subject);

            // Add action to lookup for Publish step.
            _routes.Add(type, (rawMessage) =>
            {
                var message = (T)rawMessage;

                // Using a closure here. Might be better to cache value in it's own dictionary 
                // If this proves to leak memory or have debugging issues.
                // This packages up the request with relavent client info.
                subject.OnNext(Package<T>(message));
            });
        }

        /// <summary>
        /// Returns an IObservable<ClientCommMessage<typeparamref name="T"/>> that 
        /// reacts to inbound messages of the specified type.
        /// </summary>
        /// <typeparam name="T">ICommMessage type to listen for.</typeparam>
        /// <returns>Observable sequence that reacts to inbound messages.</returns>
        public IObservable<ClientCommMessage<T>> SubscribeToType<T>() where T : ICommMessage
        {
            return _clientCommRouter.RegisterSubscriber<T>();
        }

        /// <summary>
        /// Wraps an ICommMessage with Client information, like the associated player or area.
        /// </summary>
        /// <typeparam name="T">ICommMessage type.</typeparam>
        /// <param name="message">Message to wrap.</param>
        /// <returns>ClientCommMessage instance of the specified type.</returns>
        private ClientCommMessage<T> Package<T>(T message) where T : ICommMessage
        {
            // Todo: Stubbed.
            return new ClientCommMessage<T>(message, null, null);
        }
    }
}
