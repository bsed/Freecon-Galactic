using Freecon.Client.Managers;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Freecon.Client.Objects.Projectiles
{
    public class HullPlagueProjectile : LaserProjectile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HullPlagueProjectile"/> class.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        /// <param name="spriteBatch">The sprite batch.</param>
        /// <param name="creationTime">The creation time.</param>
        /// <param name="type">The type.</param>
        /// <param name="ID">The identifier.</param>
        /// <param name="stats">The stats.</param>
        /// <param name="world">The world.</param>
        public HullPlagueProjectile(CollisionManager collisionManager,
            SpriteBatch spriteBatch, int ID, byte firingWeaponSlot, World world)
            : base(collisionManager, spriteBatch, ID, firingWeaponSlot, world)
        {
            Stats = new HullPlagueProjectileStats();
            DrawColor = Color.Fuchsia;
        }


    }
}
