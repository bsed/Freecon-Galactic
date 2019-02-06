using Freecon.Client.Managers;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework; using Core.Interfaces;
using Core.Models.Enums;
using Freecon.Client.Objects.Projectiles;
using Microsoft.Xna.Framework.Graphics;

namespace Freecon.Client.Interfaces
{
    public interface IProjectile : IPhysicsObject, ICollidable
    {
        int Id { get; set; }

        void Terminate();

        bool Enabled { get; }

        bool IsBodyValid { get; }

        CollisionDataObject BodyData { get; set; }

        ParticleEffectType TerminationEffect { get; }

        float TerminationEffectSize { get; }

        Color DrawColor { get; set; }

        void Draw(SpriteBatch spriteBatch);

        void Update(IGameTimeService gameTime);
    }

    public interface IBodyProjectile:IProjectile
    {
        Body Body { get; }

    }
}
