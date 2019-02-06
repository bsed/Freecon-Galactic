using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework; using Core.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using Freecon.Client.Managers;
using Freecon.Client.Managers.Networking;
using Freecon.Models.TypeEnums;
using Freecon.Client.Interfaces;
using Freecon.Core.Utils;

namespace Freecon.Client.Objects
{
    public class WarpHole:ICollidable
    {
        // Phyics Related
        private MessageService_ToServer _messageManager;
        private PhysicsManager _physicsManager;
        private WarpHoleManager _warpholeManager;

        public bool Enabled { get { return body.Enabled; } }

        // Rendering Related
        private readonly Texture2D WarpTexture;
        private bool Collided; // Only true when needing to warp.
        public int Id { get; protected set; }
        public int DestinationAreaID;
        public Body body;

        public float WarpRequestInterval = 200;//ms, minimum amount of time which needs to pass between warp requests
        double _lastTimeStamp;


        public WarpHole(MessageService_ToServer messageManager,
                        World world,
                        Texture2D Texture, 
                        Vector2 Position, 
            int warpIndex,
                        int destinationAreaID)
        {
            _messageManager = messageManager;

            WarpTexture = Texture;

#if DEBUG
            Debugging.AddStack.Push(this.ToString());
#endif
            body = BodyFactory.CreateCircle(world, ConvertUnits.ToSimUnits(128 / 2), 4);
            body.BodyType = BodyType.Static;

            body.Position = Position;

            body.SleepingAllowed = true;
            body.OnCollision += body_OnCollision;

            Id = warpIndex;
            DestinationAreaID = destinationAreaID;

            body.UserData = new WarpholeBodyDataObject(this, destinationAreaID);

            
           
        }

        public void Update(IGameTimeService gametime)
        {
            _lastTimeStamp = gametime.TotalMilliseconds;
        }
        
        private bool body_OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            if (((CollisionDataObject) fixtureB.Body.UserData).BodyType == BodyTypes.PlayerShip)
            {
                var ship = ((ShipBodyDataObject)fixtureB.Body.UserData).Ship;

                if (ship.CanLandWarp && ship.GetCurrentEnergy() >= 1000 && ship.EnterMode && _lastTimeStamp - ship.LastWarpRequestTime > WarpRequestInterval) //Check energy level and enter mode
                {
                    ship.LastWarpRequestTime = _lastTimeStamp;
                    ship.EnterMode = false;
                    ship.SendPositionUpdates = false;
                    ship.PositionUpdateDisableTimestamp = TimeKeeper.MsSinceInitialization;
                    _messageManager.SendWarpRequest(ship.Id, ((WarpholeBodyDataObject)fixtureA.Body.UserData).ID, ((WarpholeBodyDataObject)fixtureA.Body.UserData).DestinationAreaID);
                    
                }
            }
            return false;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(WarpTexture, ConvertUnits.ToDisplayUnits(body.Position), null,
                             Color.White, 0, new Vector2(WarpTexture.Width/2, WarpTexture.Height/2), 1,
                             SpriteEffects.None, 0.8f);

            spriteBatch.Draw(TextureManager.greenPoint, ConvertUnits.ToDisplayUnits(body.Position), null,
                             Color.White, 0, new Vector2(WarpTexture.Width / 2, WarpTexture.Height / 2), 5,
                             SpriteEffects.None, 0.8f);
        }
    }

    public enum WarpholeTypes : byte
    {
        Planet,
        Space,
        ColonyInternal,//"Warpholes" inside of a colony, to simplify colony exit

    }

}