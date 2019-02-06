using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using Freecon.Client.Managers;
using Freecon.Client.Managers.Networking;
using Freecon.Models.TypeEnums;
using Freecon.Core.Utils;

namespace Freecon.Client.Objects
{
    public class Port : Planet //WARNING:Lazy quick fix, port should not inherit from planet, fix later
        //Should probably make a generic spaceObject
    {

        public float DockRequestCooldown = 200;//TODO: throw into a config somewhere

        public Port(MessageService_ToServer messageService)
            : base(messageService)
        {
        }

        public override bool body_OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {

            if (((CollisionDataObject)fixtureB.Body.UserData).BodyType != BodyTypes.PlayerShip)
                // If either doesn't contain either
                return true;

            var ship = ((ShipBodyDataObject)fixtureB.Body.UserData).Ship;

            if (ship.CanLandWarp && 
                ship.EnterMode &&
                ship.GetCurrentEnergy() == ship.MaxEnergy &&
                TimeKeeper.MsSinceInitialization - ship.LastDockRequestTime > DockRequestCooldown)
            {

                ship.LinearVelocity *= 0.05f;
                ship.SendPositionUpdates = false;
                ship.LastDockRequestTime = TimeKeeper.MsSinceInitialization;
            }
            return true;
        }
    }
}