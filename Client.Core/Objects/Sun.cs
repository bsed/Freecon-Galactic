using Freecon.Client.Interfaces;
using Freecon.Client.Managers;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Freecon.Models.TypeEnums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Core.Models.Enums;

namespace Freecon.Client.Core.Objects
{

    public class Sun:ICollidable
    {
        public Body Body { get; set; }

        public int Id { get; set; }

        public bool Enabled { get { return Body.Enabled; } }

        public float InnerGravityStrength;

        public float OuterGravityStrength;

        public float Radius;

        public float Density;

        public SunTypes Type; // 0 for yellow, 1 for blue, add more in future

        public Texture2D Texture { get; set; }

        public Sun(World world, Texture2D texture, float radius, float density, float innerGravityStrength, float outerGravityStrength, SunTypes type)
        {

            Debugging.AddStack.Push(this.ToString());
            Body = BodyFactory.CreateCircle(world, radius, density);
            Body.UserData = new CollisionDataObject(this, BodyTypes.Sun);
            Body.BodyType = BodyType.Static;
            Body.Position = Vector2.Zero;
            Body.Restitution = 0.8f;
            Body.Friction = 0;
            Body.IsStatic = true;

            Radius = radius;
            Density = density;
            Type = type;

            InnerGravityStrength = innerGravityStrength;
            OuterGravityStrength = outerGravityStrength;

            Texture = texture;
        }

        /// <summary>
        /// Creates a new body based on the sun's density and radius
        /// </summary>
        /// <param name="w"></param>
        /// <returns></returns>
        public Body InitializeBody(World w)
        {
            Debugging.AddStack.Push(this.ToString());
            Body = BodyFactory.CreateCircle(w, Radius, Density);
            return Body;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Vector2.Zero, null,
                                 Color.White, 0, new Vector2(Texture.Width/2f, Texture.Height/2f), Radius/2,
                                 SpriteEffects.None, 0.82f);
        }       
    }
}
