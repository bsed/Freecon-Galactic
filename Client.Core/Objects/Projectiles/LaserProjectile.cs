using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework.Graphics;
using Freecon.Client.Interfaces;
using Freecon.Client.Managers;

namespace Freecon.Client.Objects.Projectiles
{

    public class LaserProjectile : RayCastProjectile<LaserProjectileStats>
    {


        /// <summary>
        /// Gets the type of the target.
        /// </summary>
        /// <value>
        /// The type of the target.
        /// </value>
        public TargetTypes TargetType
        {
            get { return TargetTypes.Moving; }
        }



        /// <summary>
        /// Initializes a new instance of the <see cref="LaserProjectile"/> class.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        /// <param name="spriteBatch">The sprite batch.</param>
        /// <param name="creationTime">The creation time.</param>
        /// <param name="type">The type.</param>
        /// <param name="ID">The identifier.</param>
        /// <param name="stats">The stats.</param>
        /// <param name="world">The world.</param>
        public LaserProjectile(CollisionManager collisionManager,
            SpriteBatch spriteBatch,
            int ID, byte firingWeaponSlot, World world)
            : base(collisionManager, spriteBatch, ID, firingWeaponSlot, world)
        {

        }



    }
}
