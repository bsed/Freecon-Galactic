using Microsoft.Xna.Framework;
using Freecon.Client.Managers;

namespace Freecon.Client.Interfaces
{
    public interface IPhysicsObject
    {

        float AngularVelocity { get; set; }

        Vector2 LinearVelocity { get; set; }

        Vector2 Position { get; set; }

        float Rotation { get; set; }

        CollisionDataObject BodyData { get; set; }


    }
}
