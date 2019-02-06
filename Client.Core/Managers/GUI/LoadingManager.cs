using System;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework; using Core.Interfaces;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ProjectMercury;
using ProjectMercury.Renderers;


namespace Freecon.Client.Managers.GUI
{
    /// <summary>
    /// This is Free's silly login screen. It's a flame ball bouncing around an arc. I'm sure few people will see it, but it's still better than a blank screen!
    /// </summary>
    public class LoadingManager
    {
        // Renderer that draws particles to screen
        // Particle effect object to store the info about particle
        private readonly Body block;
        private readonly Body bouncy;
        private readonly ParticleEffect engineEffect;
        private readonly SpriteBatchRenderer myRenderer;
        // Physics
        private readonly World world;

        private Vector2 Circle;

        //public LoadingManager(ContentManager Content, 
        //                      GraphicsDeviceManager graphics)
        //{
        //    // Create new renderer and set its graphics devide to "this" device
        //    myRenderer = new SpriteBatchRenderer
        //                     {
        //                         GraphicsDeviceService = graphics
        //                     };

        //    engineEffect = new ParticleEffect();

        //    world = new World(new Vector2(0, 1));
        //    Debugging.AddStack.Push(this.ToString());
        //    bouncy = BodyFactory.CreateCircle(world, ConvertUnits.ToSimUnits(60), 100);
        //    bouncy.Position = ConvertUnits.ToSimUnits(100, 0 + graphics.GraphicsDevice.Viewport.Height/10);
        //    bouncy.BodyType = BodyType.Dynamic;
        //    bouncy.LinearVelocity = new Vector2(0.15f, 0);
        //    Debugging.AddStack.Push(this.ToString());
        //    block = BodyFactory.CreateSolidArc(world, 100, MathHelper.ToRadians(180), 100, ConvertUnits.ToSimUnits(1000),
        //                                       ConvertUnits.ToSimUnits(graphics.GraphicsDevice.Viewport.Width/2,
        //                                                               graphics.GraphicsDevice.Viewport.Height), 0.005f);
        //    block.Restitution = 1f;
        //    // load particle effects
        //    engineEffect = Content.Load<ParticleEffect>(@"EffectLibrary\BasicFireball");
        //    engineEffect.LoadContent(Content);
        //    engineEffect.Initialise();
        //    myRenderer.LoadContent(Content);
        //}

        //public void Update(IGameTimeService gameTime)
        //{
        //    // Check if mouse left button was presed
        //    if (MouseManager.LeftButtonDown)
        //    {
        //        // Add new particle effect to mouse coordinates
        //        engineEffect.Trigger(new Vector2(MouseManager.CurrentPosition.X, MouseManager.CurrentPosition.Y));
        //    }
        //    engineEffect.Trigger(ConvertUnits.ToDisplayUnits(bouncy.Position));

        //    // "Deltatime" ie, time since last update call
        //    var SecondsPassed = (float)gameTime.ElapsedMilliseconds/1000f;
        //    engineEffect.Update(SecondsPassed);
        //    world.Step(Math.Min((float)gameTime.ElapsedMilliseconds * 0.001f, (1f / 30f)));
        //}

        //public void Draw(SpriteBatch spriteBatch, IGameTimeService gameTime)
        //{
        //    myRenderer.RenderEffect(engineEffect);
        //    spriteBatch.Begin();
        //    //textDrawingService.DrawTextToScreenLeft(spriteBatch, 1, "Logging In");
        //    spriteBatch.End();
        //}
    }
}