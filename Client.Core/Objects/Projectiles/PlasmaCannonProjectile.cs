using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Freecon.Client.Managers;
using FarseerPhysics.Dynamics;
using FarseerPhysics;

namespace Freecon.Client.Objects.Projectiles
{
    public class PlasmaCannonProjectile : LaserProjectile
    {
        /// <summary>
        /// Gets or sets the percent charge.
        /// Used to draw with the right size/intensity/whatever
        /// </summary>
        /// <value>
        /// The percent charge.
        /// </value>
        public float PercentCharge { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlasmaCannonProjectile"/> class.
        /// </summary>
        /// <param name="gameTime">The game time.</param>
        /// <param name="spriteBatch">The sprite batch.</param>
        /// <param name="creationTime">The creation time.</param>
        /// <param name="type">The type.</param>
        /// <param name="ID">The identifier.</param>
        /// <param name="stats">The stats.</param>
        /// <param name="world">The world.</param>
        /// <param name="pctCharge">The PCT charge.</param>
        public PlasmaCannonProjectile(CollisionManager collisionManager,
            SpriteBatch spriteBatch, int ID, byte firingWeaponSlot, World world, float pctCharge)
            : base(collisionManager, spriteBatch, ID, firingWeaponSlot, world)
        {
            PercentCharge = pctCharge;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var origin = new Vector2(Texture.Width / 2f, Texture.Height / 2f);

            spriteBatch.Draw(Texture, ConvertUnits.ToDisplayUnits(Position),
                             null, DrawColor, Rotation, origin, PercentCharge, SpriteEffects.None, 0);
        }
    }
}
