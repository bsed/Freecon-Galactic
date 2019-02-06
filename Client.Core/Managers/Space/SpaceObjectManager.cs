using System;
using System.Collections.Generic;
using FarseerPhysics.Controllers;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Core.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using Freecon.Client.Objects;
using Freecon.Client.Managers.Networking;
using Freecon.Models.TypeEnums;
using Core.Models.Enums;
using Freecon.Client.Core.Objects;

namespace Freecon.Client.Managers.Space
{
    public class SpaceObjectManager
    {
        // GC Objects //
        private Planet m;
        private Planet p;
        private Port port;

        protected MessageService_ToServer _messageService;

        public Sun Sun;

        private GravityController innerGravity, outerGravity;
        private ParticleManager _particleManager;
        private PhysicsManager _physicsManager;
        private SpriteBatch _spriteBatch;
        TextureManager _textureManager;

        private bool _updatePlanets;

        private int planetAmount, planetAmountToUpdate; // PlanetAmountToUpdate fixes a glitch where some planets will sit at 0,0

        public List<Planet> planetList = new List<Planet>();

        public int SizeOfSystem { get; protected set; }
        private int BaseDensity = 60;
        private double timer = 1000d;
        private float oneSecondTimer;
        private int drawRad = 700;//Radius from cam center to draw stuff

        public SpaceObjectManager(TextureManager textureManager,
                                  MessageService_ToServer messageService,
                                  SpriteBatch spriteBatch, 
                                  ParticleManager particleManager, 
                                  PhysicsManager physicsManager)
        {
            _particleManager = particleManager;
            _physicsManager = physicsManager;
            _spriteBatch = spriteBatch;
            _textureManager = textureManager;
            _messageService = messageService;

            oneSecondTimer = 0;


            
            _updatePlanets = true;
            planetAmountToUpdate = 5;

            
        }

        public Sun InitializeSun(float radius, float density, float innerGravityStrength, float outerGravityStrength, SunTypes type)
        {
            Texture2D texture;           

            Sun = new Sun(_physicsManager.World, GetSunTexture(type), radius, density, innerGravityStrength, outerGravityStrength, type);
            return Sun;
        }

        public void Reset()
        {
            foreach(var p in planetList)
            {
                Debugging.DisposeStack.Push(this.ToString());
                p.body.Dispose();
            }

            _physicsManager.World.ProcessChanges();
            planetList.Clear();

            if (Sun != null)
            {
                Sun.Body = Sun.InitializeBody(_physicsManager.World);
            }

        }

        public void Update(IGameTimeService gameTime, bool drawEnterModeParticles)
        {
         
            oneSecondTimer += gameTime.ElapsedMilliseconds; // Timer variable
            // Particle Update
            if (drawEnterModeParticles && oneSecondTimer > 2000)
            {
                for (int i = 0; i < planetList.Count; i++)
                {
                    if (planetList[i].hasMoons)
                        for (int m = 0; m < planetList[i].moonList.Count; m++)
                        {
                            _particleManager.TriggerEffect(
                                ConvertUnits.ToDisplayUnits(planetList[i].moonList[m].body.Position),
                                ParticleEffectType.GlowEffect,
                                (int)(planetList[i].moonList[m].scale * (planetList[i].moonList[m].baseTexture.Width * 2)));
                        }
                    _particleManager.TriggerEffect(ConvertUnits.ToDisplayUnits(planetList[i].body.Position),
                                                    ParticleEffectType.GlowEffect,
                                                    (int)((planetList[i].scale * planetList[i].baseTexture.Width * 2)));
                }

                oneSecondTimer = 0;
            }


            // Don't update planets every iteration, it causes a 95% increase in physics calculations if we do!
            // When we implement moving planets, we need to have them only update around once per second. 
            // Then that should only be an increase of around 1.6%
            // PlanetAmountToUpdate updates the planets 3-5 times instead of just once, because otherwise we get weird Farseer errors.
            if (_updatePlanets || planetAmountToUpdate != 0)
            {
                for (int i = 0; i < planetList.Count; i++)
                {
                    Planet p = planetList[i];

                    // Uncomment this when movement is implemented server-side
                    //p.currentTrip += 50f;
                    //if (p.currentTrip >= p.maxTrip)
                    //{
                    //    p.currentTrip = 0;
                    //}
                    p.increment = p.currentTrip/p.maxTrip;
                    if (p.maxTrip == 0)
                        p.increment = 0;

                    // Increment the angle
                    p.angle = MathHelper.ToRadians(360) * p.increment;

                    p.pos.X = ConvertUnits.ToSimUnits((float)Math.Cos(p.angle) * p.distance);
                    p.pos.Y = ConvertUnits.ToSimUnits((float)Math.Sin(p.angle) * p.distance);

                    p.body.Position = p.pos;

                    if (p.hasMoons)
                    {
                        for (int moons = 0; moons < p.moonList.Count; moons++)
                        {
                            Planet m = p.moonList[moons];

                            m.body.Position = m.pos;

                            // Uncomment this when movement is implemented server-side
                            //m.currentTrip += 50f;
                            //if (m.currentTrip >= m.maxTrip)
                            //{
                            //    m.currentTrip = 0;
                            //}
                            m.increment = m.currentTrip / m.maxTrip;


                            // Increment the angle
                            m.angle = MathHelper.ToRadians(360) * m.increment;


                            m.pos.X = ConvertUnits.ToSimUnits((float)Math.Cos(m.angle) * m.distance) + p.pos.X;
                            m.pos.Y = ConvertUnits.ToSimUnits((float)Math.Sin(m.angle) * m.distance) + p.pos.Y;
                            p.moonList[moons] = m;
                        }
                    }
                    planetList[i] = p;
                }

                if (planetAmountToUpdate != 0)
                {
                    planetAmountToUpdate--;
                }

                _updatePlanets = false; // Single planet body update.
            }
        }

        public void Draw(Vector2 CamSpot, float zoom)
        {
            //Draw the sun
            if (Vector2.Distance(CamSpot, ConvertUnits.ToDisplayUnits(Sun.Body.Position)) <
                _spriteBatch.GraphicsDevice.Viewport.Width + drawRad * 1 / zoom)
            {
                Sun.Draw(_spriteBatch);
            }

            // Draw all of the planets in the system
            for (int i = 0; i < planetList.Count; i++)
            {
                p = planetList[i];
                if (Vector2.Distance(CamSpot, ConvertUnits.ToDisplayUnits(p.body.Position)) <
                    _spriteBatch.GraphicsDevice.Viewport.Width + drawRad * 1 / zoom)
                {
                    _spriteBatch.Draw(p.baseTexture, ConvertUnits.ToDisplayUnits(p.pos), null,
                                     Color.White, 0, new Vector2(p.baseTexture.Width / 2, p.baseTexture.Height / 2), p.scale,
                                     SpriteEffects.None, 0.9f);
                    _spriteBatch.Draw(p.shadowTexture, ConvertUnits.ToDisplayUnits(p.pos), null,
                                     Color.White, p.angle,
                                     new Vector2(p.shadowTexture.Width / 2, p.shadowTexture.Height / 2), p.scale,
                                     SpriteEffects.None, 0.89f);
                }

                for (int n = 0; n < p.moonList.Count; n++)
                {
                    m = p.moonList[n];
                    if (Vector2.Distance(CamSpot, ConvertUnits.ToDisplayUnits(m.body.Position)) <
                        _spriteBatch.GraphicsDevice.Viewport.Width + drawRad * 1 / zoom)
                    {
                        _spriteBatch.Draw(m.baseTexture, ConvertUnits.ToDisplayUnits(m.pos), null,
                                         Color.White, 0, new Vector2(m.baseTexture.Width / 2, m.baseTexture.Height / 2),
                                         m.scale, SpriteEffects.None, 0.9f);

                        if (m.moon) // If you're not a port, draw me a shadow!
                        {
                            _spriteBatch.Draw(m.shadowTexture, ConvertUnits.ToDisplayUnits(m.pos), null,
                                             Color.White, p.angle,
                                             new Vector2(m.shadowTexture.Width / 2, m.shadowTexture.Height / 2), m.scale,
                                             SpriteEffects.None, 0.89f);
                        }

                        p.moonList[n] = m;
                    }
                }

                planetList[i] = p;
            }
        }

        public void CreateSinglePlanet(int distance, int maxTrip, PlanetTypes type, float currentTrip, float scale, int ID,
                                       int orbitID, bool isMoon)
        {
            var p = new Planet(_messageService);

            // Take values we know and create a planet.
            p.distance = distance;
            p.maxTrip = maxTrip;
            p.planetType = type;
            p.currentTrip = currentTrip;
            p.Id = ID;
            p.OrbitID = orbitID;
            p.scale = scale;
            p.scale *= .1f; // Make the scale below 1


            // Set information depending on if moon or not.
            if (isMoon)
            {
                // Search for planet to tell it has moons.
                for (int i = 0; i < planetList.Count; i++)
                {
                    if (planetList[i].Id == orbitID)
                    {
                        planetList[i].hasMoons = true;
                    }
                }
                p.planet = false;
                p.moon = true;
                p.increment = 1;
                p.angle = 1;
            }
            else
            {
                p.planet = true;
                p.moon = false;
                p.increment = 1;
                p.angle = 1;
                p.moonList = new List<Planet>();
            }


            // Set the texture
           if(_textureManager != null)
           {
               AssignPlanetTexture(p);
           }

            //TODO: Put size in a config file for fuck's sake
            p.body = CreatePlanetBody(150, p.scale, BaseDensity, p.Id);

            p.body.SleepingAllowed = false;
            p.body.Restitution = 0.8f;
            p.body.OnCollision +=new OnCollisionEventHandler(p.body_OnCollision);

            if (isMoon)
            {
                p.body.UserData = new CollisionDataObject(p, BodyTypes.Moon);
                // Search for planet to give it a moon.
                for (int i = 0; i < planetList.Count; i++)
                {
                    if (planetList[i].Id == orbitID)
                    {
                        p.increment = p.currentTrip/p.maxTrip;
                        if (p.maxTrip == 0)
                            p.increment = 0;

                        // Increment the angle
                        p.angle = MathHelper.ToRadians(360)*p.increment;


                        p.pos.X = ConvertUnits.ToSimUnits((float) Math.Cos(p.angle)*p.distance) + planetList[i].pos.X;
                        p.pos.Y = ConvertUnits.ToSimUnits((float) Math.Sin(p.angle)*p.distance) + planetList[i].pos.Y;

                        p.body.Position = p.pos;

                        planetList[i].moonList.Add(p);
                    }
                }
            }
            else
            {
                p.body.UserData = new CollisionDataObject(p, BodyTypes.Planet);
                // Set position
                p.increment = p.currentTrip/p.maxTrip;
                if (p.maxTrip == 0)
                    p.increment = 0;

                // Increment the angle
                p.angle = MathHelper.ToRadians(360)*p.increment;
                

                p.pos.X = ConvertUnits.ToSimUnits((float) Math.Cos(p.angle)*p.distance);
                p.pos.Y = ConvertUnits.ToSimUnits((float) Math.Sin(p.angle)*p.distance);


                p.body.Position = p.pos;

                planetList.Add(p);
            }

            if (planetAmountToUpdate <= 5)
                planetAmountToUpdate++;


            // Solves a world of issues to step the physics here.
            _physicsManager.ResyncWorldObjects();
        }

        void AssignPlanetTexture(Planet p)
        {
            switch (p.planetType)
            {
                // Normal planets
                case PlanetTypes.Ice: // Ice Planet
                    p.baseTexture = _textureManager.Ice;
                    p.shadowTexture = _textureManager.Shadow_Base300px;
                    break;
                case PlanetTypes.Desert: // Desert Planet
                    p.baseTexture = _textureManager.Desert;
                    p.shadowTexture = _textureManager.Shadow_Base300px;
                    break;

                case PlanetTypes.DesertTwo: // Desert Planet, Sprite 2
                    p.baseTexture = _textureManager.DesertTwo;
                    p.shadowTexture = _textureManager.Shadow_Base300px;
                    break;
                case PlanetTypes.Rocky: // Rocky Planet
                    p.baseTexture = _textureManager.Rocky;
                    p.shadowTexture = _textureManager.Shadow_Base300px;
                    break;
                case PlanetTypes.Barren: // Barren Planet
                    p.baseTexture = _textureManager.Barren;
                    p.shadowTexture = _textureManager.Shadow_Base300px;
                    break;

                case PlanetTypes.Gray: // Gray Planet
                    p.baseTexture = _textureManager.Gray;
                    p.shadowTexture = _textureManager.Shadow_Base300px;
                    break;

                case PlanetTypes.Red: // Red Planet
                    p.baseTexture = _textureManager.Red;
                    p.shadowTexture = _textureManager.Shadow_Base300px;
                    break;

                case PlanetTypes.ColdGasGiant: // Cold Gas Giant
                    p.baseTexture = _textureManager.ColdGasGiant1;
                    p.shadowTexture = _textureManager.Shadow_ColdGasGiant;
                    break;

                case PlanetTypes.IceGiant: // Ice Giant
                    p.baseTexture = _textureManager.IceGiant;
                    p.shadowTexture = _textureManager.Shadow_Base300px;
                    break;

                case PlanetTypes.HotGasGiant: // Hot Gas Giant
                    p.baseTexture = _textureManager.HotGasGiant;
                    p.shadowTexture = _textureManager.Shadow_Base300px;
                    break;

                case PlanetTypes.Radioactive: // Radioactive
                    p.baseTexture = _textureManager.Radioactive;
                    p.shadowTexture = _textureManager.Shadow_Base300px;
                    break;

                case PlanetTypes.OceanicLarge: // Water
                    p.baseTexture = _textureManager.OceanicLarge;
                    p.shadowTexture = _textureManager.Shadow_water;
                    break;

                case PlanetTypes.Frozen: // Frozen World
                    p.baseTexture = _textureManager.Frozen;
                    p.shadowTexture = _textureManager.Shadow_water;
                    break;

                // Small worlds
                case PlanetTypes.Crystalline: // Crystalline
                    p.baseTexture = _textureManager.Crystalline;
                    p.shadowTexture = _textureManager.Shadow_Crystalline;
                    break;

                case PlanetTypes.Gaia: // Gaia
                    p.baseTexture = _textureManager.Gaia;
                    p.shadowTexture = _textureManager.Shadow_Gaia;
                    break;
                case PlanetTypes.OceanicSmall: // Oceanic
                    p.baseTexture = _textureManager.OceanicSmall;
                    p.shadowTexture = _textureManager.Shadow_OceanicSmall;
                    break;

                // Special worlds
                case PlanetTypes.Paradise: // Paradise
                    p.baseTexture = _textureManager.Paradise;
                    p.shadowTexture = _textureManager.Shadow_Base300px;
                    break;

                case PlanetTypes.Port: // Port
                    p.baseTexture = _textureManager.Port;
                    p.shadowTexture = _textureManager.Shadow_Barren;
                    break;

                default: // Ice Planet
                    p.baseTexture = _textureManager.Ice;
                    p.shadowTexture = _textureManager.Shadow_Barren;
                    break;
            }

        }

        Texture2D GetSunTexture(SunTypes sunType)
        {
            if (_textureManager == null)
                return null;

            switch (sunType) // Pick which type of sun it is to draw the right texture
            {
                case SunTypes.Yellow:
                    return _textureManager.YellowSun;                    
                case SunTypes.Blue:
                    return _textureManager.BlueSun;                    
                case SunTypes.Orange:
                    return _textureManager.BlueSun;                    
                case SunTypes.Red:
                    return _textureManager.BlueSun;                    
                case SunTypes.White:
                    return _textureManager.BlueSun;                    
                case SunTypes.BlackHole:
                    return _textureManager.BlueSun;                    
                default:
                    return _textureManager.YellowSun;                    
            }
        }

        public void CreatePort(int distance, int maxTrip, PlanetTypes type, float currentTrip, float scale, int ID,
                               int parentID, bool isMoon)
        {
            var port = new Port(_messageService);

            // Take values we know and create a planet.
            port.distance = distance;
            port.currentTrip = currentTrip;
            port.maxTrip = maxTrip;
            port.planetType = type;
            port.Id = ID;
            port.OrbitID = parentID;
            port.scale = scale;
            port.scale *= .1f; // Make the scale below 1
            port.moonList = new List<Planet>();
            port.minimapColor = Color.Fuchsia;
            //Console.WriteLine(distance);

            // Set information depending on if moon or not.
            if (isMoon)
            {
                // Search for planet to tell it has moons.
                for (int i = 0; i < planetList.Count; i++)
                {
                    if (planetList[i].Id == parentID)
                    {
                        planetList[i].hasMoons = true;
                    }
                }
                port.planet = false;
                port.moon = true;
                port.increment = 1;
                port.angle = 1;
            }
            else
            {
                port.planet = true;
                port.moon = false;
                port.increment = 1;
                port.angle = 1;
            }


            // Set the texture
            if (_textureManager != null)
            {
                port.baseTexture = _textureManager.Port;
                port.shadowTexture = _textureManager.Port;
            }
                port.body = CreatePortBody(360 / 4f, 1f, BaseDensity, port.Id);
           
            port.body.SleepingAllowed = true;
            port.body.Restitution = 0.8f;
            port.body.OnCollision += (port.body_OnCollision);
            port.body.UserData = new CollisionDataObject(port, BodyTypes.Port);

            if (isMoon)
            {
                // Search for planet to give it a moon.
                for (int i = 0; i < planetList.Count; i++)
                {
                    if (planetList[i].Id == parentID)
                    {
                        port.increment = port.currentTrip/port.maxTrip;
                        if (maxTrip == 0)
                            port.increment = 0;

                        // Increment the angle
                        port.angle = (float)(Math.PI * 2) * port.increment;


                        port.pos.X = ConvertUnits.ToSimUnits((float)Math.Cos(port.angle) * port.distance) +
                                     planetList[i].pos.X;
                        port.pos.Y = ConvertUnits.ToSimUnits((float)Math.Sin(port.angle) * port.distance) +
                                     planetList[i].pos.Y;

                        port.body.Position = port.pos;

                        planetList[i].moonList.Add(port);
                    }
                }
            }
            else
            {
                // Set position
                port.increment = port.currentTrip / port.maxTrip;

                // Increment the angle
                port.angle = MathHelper.ToRadians(360) * port.increment;

                port.pos.X = ConvertUnits.ToSimUnits((float)Math.Cos(port.angle) * port.distance);
                port.pos.Y = ConvertUnits.ToSimUnits((float)Math.Sin(port.angle) * port.distance);

                //TODO: remove this, it's a temporary fix for a likely temporary problem
                if (port.pos.X != port.pos.X)//NaN test
                    port.body.Position = new Vector2(15, 15);
                else
                    port.body.Position = port.pos;

                planetList.Add(port);
            }

            if (planetAmountToUpdate <= 5)
                planetAmountToUpdate++;
            port.moon = false;
            port.scale = .5f;
            // Solves a world of issues to step the physics here.
            _physicsManager.ResyncWorldObjects();
        }


        private Body CreatePlanetBody(float size, float scale, int density, int ID)
        {
            // Create body
            Debugging.AddStack.Push(this.ToString());
            return BodyFactory.CreateCircle(_physicsManager.World, ConvertUnits.ToSimUnits(size * scale),
                                            ConvertUnits.ToSimUnits(density));
        }

        private Body CreatePortBody(float size, float scale, int density, int ID)
        {
            Debugging.AddStack.Push(this.ToString());
            return BodyFactory.CreateCircle(_physicsManager.World, ConvertUnits.ToSimUnits(size * scale),
                                            ConvertUnits.ToSimUnits(density));
        }


        //public int currentLayout() { return layoutNumber; } // Not implemented
    }
}
