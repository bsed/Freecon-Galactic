using FarseerPhysics;
using Microsoft.Xna.Framework;
using Core.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using Freecon.Client.Managers;
using FarseerPhysics.Dynamics;
using System.Collections.Generic;
using Freecon.Client.Interfaces;
using Freecon.Client.Objects.Weapons;
using Freecon.Models.TypeEnums;
using Freecon.Client.Core.Interfaces;
using Freecon.Client.Mathematics;

namespace Freecon.Client.Objects.Structures
{
    public class Structure : IDraw, ITargetable, ITargeter, ISimulatable, ISynchronousUpdate, ICollidable
    {

        public float xPos;
        public float yPos;
        public StructureTypes StructureType;
        public float health;
        protected Body _body { get; set; }
        
        public Texture2D Texture;
        public int Id { get; set; }
        public Weapon Weapon;
        public bool WaitingForFireResponse;
        public bool Enabled { get { return _body.Enabled; } }

        public bool IsLocalSim { get; set; }

        public float AngularVelocity
        {
            get { return _body.AngularVelocity; }
            set { if (!_body.IsDisposed) _body.AngularVelocity = value; }
        }

        public Vector2 LinearVelocity
        {
            get { return _body.LinearVelocity; }
            set { if (!_body.IsDisposed) _body.LinearVelocity = value; }
        }

        public float Mass
        {
            get { return _body.Mass; }
            set { if (!_body.IsDisposed) _body.Mass = value; }
        }

        public Vector2 Position
        {
            get { return _body.Position; }
            set { if (!_body.IsDisposed) _body.Position = value; }
        }

        public float Rotation
        {
            get { return _body.Rotation; }
            set { if (!_body.IsDisposed) _body.Rotation = value; }
        }

        public float LinearDamping
        {
            get { return _body.LinearDamping; }
            set { if (!_body.IsDisposed) _body.LinearDamping = value; }
        }
        public TargetTypes TargetType
        {
            get { return TargetTypes.Static; }
        }

        public virtual bool IsBodyValid
        {
            get { return !_body.IsDisposed; }
        }

        public bool IsDead;
         
        public HashSet<int> Teams { get; set; }

        public bool IsAlliedWithPlanetOwner
        {
            get;
            set;
        }

        public Dictionary<int, ITargetable> PotentialTargets
        {
            get;
            set;
        }

        public ITargetable CurrentTarget
        {
            get;
            set;
        }

        protected SpriteBatch _spriteBatch;

        public Structure(SpriteBatch spriteBatch, Texture2D texture, float posX, float posY, StructureTypes structureType, float health, int ID, HashSet<int> teams)
        {
            xPos = posX;
            yPos = posY;
            this.StructureType = structureType;
            this.health = health;
            this.Id = ID;

            _spriteBatch = spriteBatch;

            if (teams == null)
                Teams = new HashSet<int>();
            else
            {
                Teams = teams;
            }
            PotentialTargets = new Dictionary<int, ITargetable>();

            Texture = texture;

            //Not sure if this will ever be ITeamable

           

        }
        
        public virtual void Update(IGameTimeService gameTime)
        {

        }

        public virtual void Draw(Camera2D camera)
        {
            _spriteBatch.Draw(Texture, ConvertUnits.ToDisplayUnits(new Vector2(xPos, yPos)), Color.White);
        }

        public virtual void Kill()
        {
            DisposeBodies();
            IsDead = true;
        }

        public void SetBody(Body b)
        {
            if(_body != null && !_body.IsDisposed)
                _body.Dispose();

            _body = b;
        }

        public virtual void DisposeBodies()
        {
            if (_body != null)
            {
                _body.Enabled = false;
                Debugging.DisposeStack.Push(this.ToString());
                _body.Dispose();
            }

        }

        public virtual void Simulate(IGameTimeService gametime)
        {
            //Probably do some cool graphics stuff here or something.
        }
    }

}
