using System.Collections.Generic;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Freecon.Client.Objects
{
    // Temporary placeholder in memory for recieved objects.
    public class QueuedPlanet
    {
        public float angle;
        public Texture2D baseTexture;
        public Body body;
        public float currentTrip;
        public int distance;
        public bool hasMoons;
        public int howManyMoons;
        public float increment; // something is setting it to zero, its that division shit.
        public int layout;
        // Rotated
        public int maxTrip;
        public bool moon;
        // Subobjects
        public List<Planet> moonList;
        public bool planet;
        public int planetType;
        public Vector2 pos;
        public float scale;
        public Texture2D shadowTexture;

        public QueuedPlanet(int distance, int maxTrip, int currentTrip, float scale, int planetType, byte parent,
                            byte ID)
        {
            this.distance = distance;
            this.maxTrip = maxTrip;
            this.currentTrip = currentTrip;
            this.scale = scale;
            this.planetType = planetType;
            this.currentTrip = currentTrip;
        }
    }
}