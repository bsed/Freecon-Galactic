using Core.Models.Enums;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Core.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using Freecon.Client.Interfaces;
using Freecon.Client.Managers;
using Freecon.Client.Managers.Networking;
using Freecon.Core.Networking.Models.Messages;
using Freecon.Core.Utils;

namespace Freecon.Client.Core.Objects
{
    public class FloatyAreaObject: ICollidable
    {
        public Texture2D DrawTex { get; set; }

        public Color DrawColor { get; set; }

        public bool Enabled { get { return Body.Enabled; } }

        public Vector2 Position { get { return Body.Position; } set { Body.Position = value; } }

        public float Rotation { get { return Body.Rotation; } set { Body.Rotation = value; } }

        public FloatyAreaObjectTypes Type { get; set; }

        float _lastSendTime = 0;

        public Body Body { get; protected set; }

        protected MessageService_ToServer _messageService;

        public int Id { get; protected set; }

        public FloatyAreaObject(int galaxyID, World world, MessageService_ToServer messageService, FloatyAreaObjectTypes type, Texture2D drawTex, Vector2 position, float rotation)
        {
            Id = galaxyID;
            _messageService = messageService;

            DrawColor = Color.Lime;
            Type = type;

            
            DrawTex = drawTex;

            CreateBody(world, position, rotation);

            
        }


        protected virtual void CreateBody(World w, Vector2 position, float rotation)
        {
            Debugging.AddStack.Push(this.ToString());
            Body = BodyFactory.CreateCircle(w, .1f, 1);
            Body.BodyType = BodyType.Static;
            Body.IsBullet = true;
            Body.Friction = 0;
            Body.Restitution = 0.5f;
            Body.LinearDamping = 0.00001f;
            Body.Position = position;
            Body.Rotation = rotation;
            Body.UserData = new FloatyAreaBodyDataObject(this);

            Body.OnCollision += _body_OnCollision;
        }

        protected virtual bool _body_OnCollision(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            if (fixtureB.UserData is ShipBodyDataObject)
            {
                if (TimeKeeper.MsSinceInitialization - _lastSendTime > 1000)
                {
                    _messageService.SendObjectPickupRequest(((ShipBodyDataObject)fixtureB.UserData).ID, Id, PickupableTypes.FloatyAreaObject);
                    _lastSendTime = TimeKeeper.MsSinceInitialization;
                    
                }
            }

            return false;
        }



        public virtual void Update(IGameTimeService gameTime)
        { 
        
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            var drawpos = ConvertUnits.ToDisplayUnits(Body.Position);

            spriteBatch.Draw(DrawTex, ConvertUnits.ToDisplayUnits(Body.Position),
                             new Rectangle(0, 0, DrawTex.Width, DrawTex.Height),
                             DrawColor, Body.Rotation,
                             new Vector2(DrawTex.Width / 2f, DrawTex.Height / 2f),
                             10, SpriteEffects.None, 0.2f);
        
        }

        public virtual void Dispose()
        {
#if DEBUG
            Debugging.DisposeStack.Push(this.ToString());
#endif
            Body.Dispose();
        }
    }
}
