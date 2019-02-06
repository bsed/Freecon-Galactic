using System;
using Freecon.Client.Core.Behaviors;

namespace Freecon.Client.Interfaces
{
    public interface IShip : IPhysicsObject
    {
        int GetCurrentEnergy();

        void SetCurrentEnergy(int value);

        event EventHandler<BodyPositionUpdatedEventArgs> BodyPositionUpdated;
    }
}
