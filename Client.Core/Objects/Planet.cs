using System.Collections.Generic;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using FarseerPhysics.Dynamics.Contacts;
using Freecon.Client.Managers;
using Freecon.Client.Managers.Networking;
using Freecon.Models.TypeEnums;
using System;
using Freecon.Client.Interfaces;
using Freecon.Core.Utils;

namespace Freecon.Client.Objects
{
    public class Planet:ICollidable
    {
        public int Id { get; set; }
        public float angle;
        public Texture2D baseTexture;
        public Body body;
        public float currentTrip;
        public int distance;
        public int gravity;
        public bool hasMoons;
        public int howManyMoons;
        public float increment; // something is setting it to zero, its that division shit.
        public int layoutNumber;
        public int mass;
        public int maxTrip;
        public Color minimapColor = Color.White;
        public bool moon;
        public List<Planet> moonList;

        // Identification for Networking
        public int OrbitID;
        public bool planet;
        public PlanetTypes planetType;
        public Vector2 pos;
        public float scale;
        public Texture2D shadowTexture;

        public float LandRequestCooldown = 200;//TODO: add to some kind of config class

        public bool Enabled { get { return body.Enabled; } }

        protected MessageService_ToServer _messageService;


        public Planet(MessageService_ToServer messageService)
        {
            _messageService = messageService;
        }

        public virtual bool body_OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            //string s = (string)fixtureB._body.UserData; // FixtureB is the colliding fixture.
            try
            {


                if (((CollisionDataObject)fixtureB.Body.UserData).BodyType != BodyTypes.PlayerShip)
                    // If either doesn't contain either
                    return true;
            }
            catch(Exception e)
            {
                Console.WriteLine("ffijs1 something's null...");
            }

            var ship = ((ShipBodyDataObject)fixtureB.Body.UserData).Ship;

            if (ship.EnterMode &&
                ship.GetCurrentEnergy() == ship.MaxEnergy &&
                TimeKeeper.MsSinceInitialization - ship.LastLandRequestTime > LandRequestCooldown
                )
            {

                ship.LinearVelocity = ship.LinearVelocity * 0.05f;
                ship.EnterMode = false;
                _messageService.SendLandRequest(ship.Id, Id);
                ship.SendPositionUpdates = false;
                ship.PositionUpdateDisableTimestamp = TimeKeeper.MsSinceInitialization;
                ship.LastLandRequestTime = TimeKeeper.MsSinceInitialization;

            }
            return true;
        }
    }
}