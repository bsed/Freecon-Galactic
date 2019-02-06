using System;
using System.Collections.Generic;
using Freecon.Client.Extensions;
using Freecon.Client.Interfaces;
using Freecon.Client.Managers;
using Freecon.Client.Managers.Networking;
using Freecon.Client.Objects.Structures;
using Freecon.Client.Objects.Weapons;
using Core.Interfaces;
using Core.Models.Enums;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Freecon.Client.Mathematics;
using Freecon.Core.Networking.Models.Messages;
using Freecon.Models.TypeEnums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Freecon.Core.Utils;

namespace Freecon.Client.Core.Objects.Invasion
{
    public class DefensiveMine : Structure, ITeamable, ICanFire
    {

        private MessageService_ToServer _messageService;

        public float Rotation { get; private set; }

        World _world;

        public HashSet<int> Teams { get; set; }

        public ParticleEffectType TerminationEffect;

        protected float _blinkOnDuration = 100;
        protected float _blinkOffDuration = 1000;
        protected float _blinkOffset;


        //TODO: put in config file
        protected int _drawWidth = 100;//Pixels
        protected int _drawHeight = 100;//Pixels

        private Texture2D _blinkOnTex;
        private Texture2D _blinkOffTex;
                
        private Rectangle _destinationRectangle;

        bool _isTriggered;
        float _fuseDelay = 100;//TODO: put in config file. Delay to hit ships which are moving toward the mine with more damage
        float _triggerTime;

        /// <summary>
        /// Smaller body which detects allied objects attempting to pick up the mine
        /// </summary>
        protected Body _pickupBody;


        protected static Random r = new Random();

        public DefensiveMine(World w, SpriteBatch spriteBatch, int ID, MessageService_ToServer messageService, HashSet<int> teamIDs, Vector2 position, float rotation, Texture2D blinkOffTex, Texture2D blinkOnTex, ProjectileManager pm)
            : base(spriteBatch, blinkOffTex, position.X, position.Y, StructureTypes.DefensiveMine, 100, ID, teamIDs)
        {

            _blinkOnTex = blinkOnTex;
            _blinkOffTex = blinkOffTex;

            TerminationEffect = ParticleEffectType.ExplosionEffect;
            _messageService = messageService;
            
            CreateBodies(w, position, rotation);

            Weapon = new MineWeapon(pm, this, 0);

            Teams = teamIDs;
            _world = w;

            
            


            //Randomly stagger blinking
            _blinkOffset = r.Next(0, (int)_blinkOffDuration) + TimeKeeper.MsSinceInitialization;

        }


        protected void CreateBodies(World w, Vector2 position, float rotation)
        {
            Debugging.AddStack.Push(this.ToString());
            _body = BodyFactory.CreateCircle(w, 1.1f, 1);
            _body.BodyType = BodyType.Static;
            _body.IsBullet = true;
            _body.Friction = 0;
            _body.Restitution = 0.5f;
            _body.LinearDamping = 0.00001f;
            _body.Position = position;
            _body.Rotation = rotation;
            _body.UserData = new StructureBodyDataObject(BodyTypes.DefensiveMine, this);
            _body.OnCollision += _body_OnCollision;

            //Note: radius of the pickup body is intentionally smaller, should match texture, while the detonation body might be larger
            Debugging.AddStack.Push(this.ToString());
            _pickupBody = BodyFactory.CreateCircle(w, 1f, 1);
            _pickupBody.BodyType = BodyType.Static;
            _pickupBody.IsBullet = true;
            _pickupBody.Friction = 0;
            _pickupBody.Restitution = 0.5f;
            _pickupBody.LinearDamping = 0.00001f;
            _pickupBody.Position = position;
            _pickupBody.Rotation = rotation;
            _pickupBody.UserData = new StructureBodyDataObject(BodyTypes.DefensiveMine, this);
            _pickupBody.OnCollision += _pickupBody_OnCollision;
        }

        protected bool _body_OnCollision(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {

            if (fixtureB.Body.UserData is ShipBodyDataObject)
            {
                var s = fixtureB.Body.UserData as ShipBodyDataObject;

                if (!s.Ship.OnSameTeam(this))
                {
                    Trigger();
                }
               
            }

            return false;
        }

        protected bool _pickupBody_OnCollision(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {

            if (fixtureB.Body.UserData is ShipBodyDataObject)
            {
                var s = fixtureB.Body.UserData as ShipBodyDataObject;

                if (s.Ship.EnterMode && s.Ship.OnSameTeam(this) && s.Ship.Cargo.CheckCargoSpace(StatefulCargoTypes.DefensiveMine, 1))
                {
                    _messageService.SendObjectPickupRequest(s.ID, Id, PickupableTypes.DefensiveMine);                   
                }
               
            }

           
            return false;
        }


        public override void Update(IGameTimeService gameTime)
        {
            base.Update(gameTime);


            if ((TimeKeeper.MsSinceInitialization - _blinkOffset) % (_blinkOffDuration + _blinkOnDuration) > _blinkOffDuration)
            {
                Texture = _blinkOnTex;
            }
            else
            {
                Texture = _blinkOffTex;
            }

            if(_isTriggered && TimeKeeper.MsSinceInitialization - _triggerTime > _fuseDelay)
            {
                Weapon.Fire_LocalOrigin(Rotation, 0, false);
            }

        }

         public override void Draw(Camera2D camera)
        {

            _destinationRectangle = new Rectangle((int)ConvertUnits.ToDisplayUnits(Position.X)-_drawWidth/2, (int)ConvertUnits.ToDisplayUnits(Position.Y)-_drawHeight/2, (int)(_drawWidth), (int)_drawHeight);
           
            _spriteBatch.Draw(Texture, _destinationRectangle, null,
                             Color.White, _body.Rotation,
                             //new Vector2(_drawWidth/ 2, _drawHeight / 2),
                             Vector2.Zero,
                             SpriteEffects.None, 0.2f);
        
        }

        
       
        public void ChangeEnergy(int amount)
        {
            return;
        }

        public float BodyHeight { get; private set; }
        public int GetCurrentEnergy()
        {
            return 999999999;
        }

        public void SetCurrentEnergy(int value)
        {
            return;
        }


        public override void Kill()
        {
            

            DisposeBodies();
        }

        public override void DisposeBodies()
        {
            base.DisposeBodies();

            if (_pickupBody != null)
            {
                _pickupBody.Enabled = false;
                Debugging.DisposeStack.Push(this.ToString());
                _pickupBody.Dispose();
            }
        }



        /// <summary>
        /// Sends fire request to server.
        /// </summary>
        public void Trigger()
        {
            if(!_isTriggered)
            {
                _triggerTime = TimeKeeper.MsSinceInitialization;
                _isTriggered = true;
            }
            

        }
    }

}






