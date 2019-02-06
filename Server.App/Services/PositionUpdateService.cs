using Freecon.Core.Networking.Models.Objects;
using Freecon.Server.Core;
using Freecon.Server.Core.Reactive;
using System;

namespace Freecon.Server.App
{
    /// <summary>
    /// A beautiful stub. :P
    /// </summary>
    public class PositionUpdateService
    {
        public PositionUpdateService(ClientCommRouter router)
        {
            router.RegisterSubscriber<PositionUpdate>().Subscribe(
                HandlePositionUpdateStream,
                (ex) => { },
                () => { }
            );
        }

        private void HandlePositionUpdateStream(ClientCommMessage<PositionUpdate> message)
        {
            var posUpdate = message.Message;
            Console.WriteLine("Got a position Update");
        }
    }
}
