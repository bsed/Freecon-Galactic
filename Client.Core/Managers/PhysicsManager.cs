using System;
using System.Collections.Generic;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Core.Interfaces;
using FarseerPhysics.Controllers;
using Freecon.Client.Core.Interfaces;


namespace Freecon.Client.Managers
{
    public class PhysicsManager : ISynchronousUpdate
    {

        public World World { get; set; }

        public Body SunBody;


        bool _isUpdating;

        /// <summary>
        /// Creates a new world.
        /// </summary>
        public PhysicsManager()
        {

            World = new World(Vector2.Zero);

#if DEBUG
            Debugging.PhysicsManager = this;
#endif
        }

        /// <summary>
        /// Steps the world.
        /// </summary>
        public void Update(IGameTimeService gameTime)
        {
            if(_isUpdating)
            {
                Console.WriteLine("Already updating!");
            }
            _isUpdating = true;
            try
            {
                World.Step(Math.Min((float)gameTime.ElapsedMilliseconds * 0.001f, (1f / 30f)));
            }
            catch(Exception e)
            {
                Console.WriteLine("WTF bro?");
            }
            
            _isUpdating = false;    
        }

        /// <summary>
        /// Deletes all bodies from world.
        /// </summary>
        private void _clearWorld(HashSet<Body> bodiesToSave)
        {

            //World = new World(new Vector2(0, 0));
            
            foreach(Body b in World.BodyList)
            {
                if (!bodiesToSave.Contains(b))
                {
                    Debugging.DisposeStack.Push(this.ToString());
                    b.Dispose();
                }
            }
            List<Controller> controllersToRemove = new List<Controller>(World.ControllerList);
            foreach(var c in controllersToRemove)
            {
                World.RemoveController(c);
            }
            World.ProcessChanges();
        }

        public void RemoveBody(Body body)
        {
            Debugging.DisposeStack.Push(this.ToString());
            body.Dispose();
        }

        public void ResyncWorldObjects()
        {
            World.Step(0);
        }

        public void Reset(Body playerShipBody)
        {
            HashSet<Body> bodiesToSave = new HashSet<Body>();
            if(playerShipBody != null)
                bodiesToSave.Add(playerShipBody);


            //if (SunBody != null)
            //    bodiesToSave.Add(SunBody);


            _clearWorld(bodiesToSave);
           
        }
    }
}