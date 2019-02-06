using Freecon.Core.Networking.Models;
using Freecon.Server.Core.Reactive;
using Reactive.Router;
using System;

namespace Freecon.Server.Core
{
    public class ClientCommRouter : Freecon.Server.Core.IClientCommRouter
    {
        private IReactiveRouter _reactiveRouter;

        public ClientCommRouter(IReactiveRouter reactiveRouter)
        {
            _reactiveRouter = reactiveRouter;
        }

        public IObservable<ClientCommMessage<T>> RegisterSubscriber<T>() where T : ICommMessage
        {
            return _reactiveRouter.RegisterSubscriber<ClientCommMessage<T>>();
        }

        public void RegisterPublisher<T>(IObservable<ClientCommMessage<T>> publishStream) where T : ICommMessage
        {
            _reactiveRouter.RegisterPublisher<ClientCommMessage<T>>(publishStream);
        }
    }
}
