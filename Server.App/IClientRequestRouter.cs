using Freecon.Core.Networking.Models;
using Freecon.Core.Networking.Models.Objects;
using Freecon.Server.Core.Reactive;
using System;

namespace Freecon.Server.App
{
    public interface IClientRequestRouter
    {
        void Publish(ICommMessage message);

        void RegisterType<T>(TodoMessageTypes type) where T : ICommMessage;

        IObservable<ClientCommMessage<T>> SubscribeToType<T>() where T : ICommMessage;
    }
}
