using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using FarseerPhysics.Collision;
using FarseerPhysics.Controllers;
using FarseerPhysics.ConvertUnits;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Common;
using SRClient.Managers;
using SRClient.Objects;
using SRClient.GUI;
using SRClient.Managers.States;


using ProjectMercury;
using ProjectMercury.Emitters;
using ProjectMercury.Modifiers;
using ProjectMercury.Renderers;


namespace SRClient.Managers
{
    public class MissileManager
    {

        #region Textures

        private Texture2D tex_Ambassador;
        private Texture2D tex_HellHound;

        #endregion

        private ParticleEffect missileEffect;
        private ParticleEffect explosionEffect;
        private SpriteBatchRenderer renderer;

        //list for the missiles
        //private static List<QueuedMissile> queueList;
        //private List<Missile> missileList = new List<Missile>();
       //private Stack<int> missileSlots = new Stack<int>();
        //private int missileAmount = 60, slotNumber;
        //private Missile m, baseMissile;

//        public MissileManager(ContentManager Content, ParticleEffect engineEffect, Renderer renderer, GraphicsDeviceManager graphics)
//        {
//            queueList = new List<QueuedMissile>();
//            this.renderer = new SpriteBatchRenderer
//            {
//                GraphicsDeviceService = graphics
//            };
//            // Loading of all the missiles.
//            #region Missile Content Loading
//            missileEffect = new ParticleEffect();
//            explosionEffect = new ParticleEffect();

//            missileEffect = Content.Load<ParticleEffect>(@"EffectLibrary\MissileEngines");
//            explosionEffect = Content.Load<ParticleEffect>(@"EffectLibrary\Explosion");

//            explosionEffect.Initialise();
//            explosionEffect.LoadContent(Content);

//            missileEffect.Initialise();
//            missileEffect.LoadContent(Content);

//            renderer.LoadContent(Content); 

//            //Load the textures (all the loading can/will be done in the first load screen of the game when we make it).
//            tex_Ambassador = Content.Load<Texture2D>(@"Weapons/Ambassador");
//            tex_HellHound = Content.Load<Texture2D>(@"Weapons/Hellhound");
//            #endregion
//            // Object pooling initialization

//            baseMissile = new Missile();
//            Vector2 vel = Vector2.Zero;
            
//            baseMissile.body = BodyFactory.CreateCircle(PhysicsManager.world, ConvertUnits.ToSimUnits(30), ConvertUnits.ToSimUnits(50));
            
//            baseMissile.body.BodyType = BodyType.Dynamic;
//            //baseMissile.body.FixedRotation = true;
//            baseMissile.body.Position = new Vector2(1000, 1000);
//            baseMissile.body.CollisionCategories = Category.Cat4;
//            baseMissile.body.CollidesWith = Category.Cat1;
//            baseMissile.body.SleepingAllowed = true;
//            baseMissile.body.Enabled = false;
//            baseMissile.body.Awake = false;

//            for (int i = 0; i < missileAmount; i++)
//            {
//                m = new Missile();
//                m.rand = Main.rand;
//                m.body = baseMissile.body.DeepClone();
//                m.body.Mass = 1f;
//                m.body.FixedRotation = false;
//                m.body.BodyType = BodyType.Dynamic;
//                m.body.IgnoreCollisionWith(baseMissile.body); // Missiles don't collide with each other.

//                m.body.IsStatic = false;
//                m.body.Mass = ConvertUnits.ToSimUnits(1f);
//                m.body.Restitution = 1f;
//                m.body.SleepingAllowed = true;
//                m.body.Enabled = false;
//                m.body.Awake = false;

//                m.body.Position = new Vector2(1000+i, 1000+i);
//                vel.X = 0;
//                vel.Y = 0;
//                m.body.LinearVelocity = vel;

//                m.body.UserData = ("Missile " + i);

//                m.body.OnCollision += new OnCollisionEventHandler(body_OnCollision);
//                m.isActive = false;

//                missileSlots.Push(i);
//                missileList.Add(m);
//            }
//        }

//        public virtual void Update(GameTime gameTime, Vector2 shipPos, float rotation)
//        {
            


//        }


//        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
//        {
//            renderer.RenderEffect(missileEffect, spriteBatch);
//            renderer.RenderEffect(explosionEffect, spriteBatch);
//            for (int i = 0; i < missileList.Count; i++)
//            {
//                if (missileList[i].isActive) // Only draw missiles that are active.
//                {
//                    if (m.turning)
//                        spriteBatch.Draw(missileList[i].tex, ConvertUnits.ToDisplayUnits(missileList[i].body.Position), null,
//                            Color.White, missileList[i].rotation, 
//                            new Vector2(missileList[i].tex.Width / 2, missileList[i].tex.Height / 2), 1, SpriteEffects.None, 0.8f);
//                    else
//                        spriteBatch.Draw(missileList[i].tex, ConvertUnits.ToDisplayUnits(missileList[i].body.Position), null,
//                            Color.Cyan, missileList[i].rotation, 
//                            new Vector2(missileList[i].tex.Width / 2, missileList[i].tex.Height / 2), 1, SpriteEffects.None, 0.8f);

//                }
//            }
//        }


//        public static void CreateQueuedMissile(Vector2 pos, float rotation, int type)
//        {
//            QueuedMissile q = new QueuedMissile(pos, rotation, type);
//            queueList.Add(q);
//        }
        
//        /// <summary>
//        /// Creation of Missles, type must be specified.
//        /// </summary>
//        /// <param name="shipPos">Position of ship to fire missile from.</param>
//        /// <param name="rotation">Rotation of Missile</param>
//        /// <param name="name">Name of Missile</param>
//        public void CreateMissile(Vector2 positionOfSpawn, float rotation, int type)
//        {
//            if (missileSlots.Count() > 0) // If we have spare missiles in memory
//                slotNumber = missileSlots.Pop();
//            else // If there are no missiles, create one.
//            {
//                m = new Missile();
//                m.rand = Main.rand;
//                m.body = baseMissile.body.DeepClone();
//                m.body.Mass = 1f;
//                m.body.FixedRotation = false;
//                m.body.BodyType = BodyType.Dynamic;
//                m.body.IgnoreCollisionWith(baseMissile.body); // Missiles don't collide with each other.

//                m.body.IsStatic = false;
//                m.body.Mass = ConvertUnits.ToSimUnits(1f);
//                m.body.Restitution = 1f;
//                m.body.SleepingAllowed = true;
//                m.body.Enabled = false;
//                m.body.Awake = false;

//                m.body.Position = new Vector2(1000 + missileList.Count(), 1000 + missileList.Count());
//                m.body.LinearVelocity = Vector2.Zero;
//                m.body.UserData = ("Missile " + missileList.Count());
//                m.turning = false;

//                m.body.OnCollision += new OnCollisionEventHandler(body_OnCollision);
//                m.isActive = false;

//                missileSlots.Push(missileList.Count());
//                missileList.Add(m);
//                slotNumber = missileSlots.Pop();
//            }
//            m = missileList[slotNumber];
//            if (type == 1)
//            {
//                m.tex = tex_Ambassador;
//                m.life = 500;
//                m.rotation = 0;
//                m.rotationIntital = rotation;
//                m.thrust = 1.1f;
//                m.thrustMax = m.thrust;
//                m.thrustModifier = .00001f;
//                m.archAngleMax = .01f;
//                m.speed = 2f;
//                m.maxSpeed = ConvertUnits.ToSimUnits(m.thrustMax);
//                m.pos = Vector2.Zero;
//                m.body.Position = positionOfSpawn;
//                m.body.Rotation = ConvertUnits.ToSimUnits(MathHelper.ToRadians(m.rotation));
//                //m.body.CollisionCategories = Category.Cat4;
//                m.body.IsBullet = true;
//                m.wobbleDifference = 20;
//                m.turnSpeed = .5f;
//                m.body.UserData = "Missile" + slotNumber;
//                m.isActive = true;

//                // Enable body
//                m.body.Enabled = true;
//                m.body.Awake = true;
//            }

//            if (type == 0)
//            {
//                //m.ObjList = new List<ObjInView>();
//                m.tex = tex_HellHound;
//                m.life = 320;
//                m.rotation = 0;
//                m.rotationIntital = rotation;
//                m.thrust = .9f;
//                m.thrustMax = m.thrust;
//                m.archAngleMax = .02f;
//                m.thrustModifier = .4f;
//                m.speed = 2f;
//                m.pos = Vector2.Zero;
//                m.body.Position = Vector2.Zero;
//                m.body.Rotation = ConvertUnits.ToSimUnits(MathHelper.ToRadians(m.rotation));
//                //.body.CollisionCategories = Category.Cat4;
//                m.body.IsBullet = true;
//                m.wobbleDifference = 20;
//                m.turnSpeed = .5f;
//                m.body.UserData = "Missile" + slotNumber;
//                m.isActive = true;

//                // Enable Body
//                m.body.Enabled = true;
//                m.body.Awake = true;
//            }
//            missileList[slotNumber] = m;
//        }

//        bool body_OnCollision(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
//        {
//            if (fixtureA.Body.UserData != null && fixtureB.Body.UserData != null)
//            {
//#if DEBUG
//                Console.WriteLine("COLLISION WITH: " + fixtureB.Body.UserData.ToString());
//#endif
//                if (fixtureB.Body.UserData.ToString() == "PlayerShip")
//                {
//                    string ID = fixtureA.Body.UserData.ToString().Remove(0, 7);
//                    int IDNum = int.Parse(ID);
//#if DEBUG
//                    Console.WriteLine(ID);
//#endif
//                    Missile m = missileList[IDNum];
//                    m.life = 0;
//                    return false;
//                }
	
//                if (fixtureB.Body.UserData.ToString().StartsWith("Planet"))
//                {
//                    string ID = fixtureA.Body.UserData.ToString().Remove(0, 7);
//                    int IDNum = int.Parse(ID);
//#if DEBUG
//                    Console.WriteLine(ID);
//#endif
//                    Missile m = missileList[IDNum];
//                    m.life = 0;
//                    return false;
//                }
//                if (fixtureB.Body.UserData.ToString().StartsWith("NetworkShip"))
//                {
//                    string ID = fixtureA.Body.UserData.ToString().Remove(0, 7);
//                    int IDNum = int.Parse(ID);
//#if DEBUG
//                    Console.WriteLine(ID);
//#endif
//                    Missile m = missileList[IDNum];
//                    m.life = 0;
//                    return false;
//                }
//            }
//            return true;
//        }
        
        
    }
}
