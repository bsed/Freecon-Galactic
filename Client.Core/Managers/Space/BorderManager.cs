using System;
using System.Collections.Generic;
using System.Linq;

using FarseerPhysics.Common;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Freecon.Client.Mathematics.Space;
using Freecon.Models.TypeEnums;
using Freecon.Client.Interfaces;

namespace Freecon.Client.Managers.Space
{
    public class BorderManager
    {
        // Textures

        public List<Vector2> borderList = new List<Vector2>();
        public int sizeOfSystem;
        private readonly Texture2D tex_EdgeOfSystem;
        private PhysicsManager _physicsManager;
        private SpriteBatch _spriteBatch;
        private int drawRad = 400;

        // Loopshape _body
        private PlanetBorder Border;

        public BorderManager(Texture2D borderTexture, SpriteBatch spriteBatch, PhysicsManager physicsManager)
        {
            _physicsManager = physicsManager;
            _spriteBatch = spriteBatch;
            Border = new PlanetBorder();
            // Debug Textures
            tex_EdgeOfSystem = borderTexture;
        }

        public void Reset(int SizeOfSystem)
        {
            Vertices verts = Boundaries.CreateConcaveSemiCircle(ConvertUnits.ToSimUnits(SizeOfSystem), SizeOfSystem / 100);

            foreach (Vector2 v in verts)
                borderList.Add(v);

            //var tempbd = new CollisionDataObject(BodyTypes.WallTile, 0);
            //Border = BodyFactory.CreateLoopShape(_physicsManager.World, verts, Vector2.Zero, tempbd);
            //Border.IsStatic = true;
            //Border.UserData = tempbd;
            //Border.SleepingAllowed = true;
            //Border.Friction = 20000f;
            //Border.Restitution = 0.2f;

            //if (Border.FixtureList == null)
            //    Console.WriteLine("Why is this happening?");
        }

        public void UpdateBorder(int SizeOfSystem)
        {
            sizeOfSystem = SizeOfSystem;
            //Console.WriteLine("Setting size of system to " + SizeOfSystem);
            if (Border.Body != null && _physicsManager.World.BodyList.Contains(Border.Body))
            {
                Debugging.DisposeStack.Push(this.ToString());
                Border.Body.Dispose();
            }
            borderList.Clear();

            Vertices verts = Boundaries.CreateConcaveSemiCircle(SizeOfSystem/100f, SizeOfSystem/100);

            foreach (Vector2 v in verts)
                borderList.Add(v);



            Border = new PlanetBorder();
            var tempbd = new CollisionDataObject(Border, BodyTypes.WallTile);
            Debugging.AddStack.Push(this.ToString());
            Border.Body = BodyFactory.CreateLoopShape(_physicsManager.World, verts, Vector2.Zero, tempbd);
            Border.Body.IsStatic = true;
            Border.Body.UserData = tempbd;
            Border.Body.SleepingAllowed = true;
            Border.Body.Friction = 0f;
            Border.Body.Restitution = 0.2f;
        }

        public void Draw(Vector2 CamSpot, float zoom)
        {
            for (int v = 0; v < borderList.Count(); v++)
                if (Vector2.Distance(CamSpot, ConvertUnits.ToDisplayUnits(borderList[v])) <
                    _spriteBatch.GraphicsDevice.Viewport.Width + drawRad * 1 / zoom)
                {
                    if (v + 1 < borderList.Count())
                    {
                        float angle =
                            MathHelper.ToRadians(
                                (float)Math.Atan2(borderList[v + 1].Y - borderList[v].Y, borderList[v + 1].X - borderList[v].X) *
                                (float)(180 / Math.PI) + 90);
                        _spriteBatch.Draw(tex_EdgeOfSystem, ConvertUnits.ToDisplayUnits(borderList[v]), null,
                                         Color.White, angle,
                                         new Vector2(tex_EdgeOfSystem.Width / 2, tex_EdgeOfSystem.Height), 1,
                                         SpriteEffects.None, 0.9f);
                    }
                }
        }

        public class PlanetBorder:ICollidable
        {
            public Body Body { get; set; }

            public bool Enabled { get { return Body.Enabled; } }

            public int Id { get; set; }

        }
    }
}