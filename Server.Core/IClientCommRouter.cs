using Freecon.Core.Networking.Models;
using Freecon.Server.Core.Reactive;
using System;

namespace Freecon.Server.Core
{

    public interface IClientCommRouter
    {
        IObservable<ClientCommMessage<T>> RegisterSubscriber<T>() where T : ICommMessage;

        void RegisterPublisher<T>(IObservable<ClientCommMessage<T>> publishStream) where T : ICommMessage;
    }
}
