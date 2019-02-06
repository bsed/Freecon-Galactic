using System;
using Freecon.Models.TypeEnums;

namespace Server.Models
{
    public class SpaceObject
    {
        // Identification
        public int ID;
        public int currentTrip;
        public int distance;
        public float gravity;
        public bool hasMoons;
        public int mass;
        public int maxTrip;
        public int objectType;
        public int parentID;
        public int planetType;

        // Object
        public int size;
        public int xPos;
        public int yPos;

        public SpaceObject()
        {
        }

        public SpaceObject(string type, int numberOrbit, int parent, int ID, int randomSeed)
        {
            var r = new Random(randomSeed);
            switch (type.ToLower())
            {
                case "sun":
                    objectType = (int) SpaceObjectType.Sun;
                    size = 1;
                    mass = 12;
                    gravity = 1;
                    break;
                case "planet":
                    objectType = (int) SpaceObjectType.Planet;

                    this.ID = ID;

                    mass = 12;
                    gravity = 0.5f;

                    // If 0, no moons. If 1, has moons.
                    int getRandomBool = r.Next(0, 10);
                    if (getRandomBool > 4)
                        hasMoons = false;
                    else
                        hasMoons = true;

                    int minDist = 700;
                    distance = (minDist + (300*numberOrbit) + r.Next(250, 400));

                    maxTrip = distance*distance;
                    currentTrip = r.Next(0, maxTrip); // Set random angle

                    size = r.Next(4, 10); // Generate a scale
                    planetType = r.Next((int) PlanetTypes.Ice, (int) PlanetTypes.Barren); // Generate a planet type 

                    parentID = parent;

                    break;
                case "moon":
                    objectType = (int) SpaceObjectType.Moon;

                    this.ID = ID;

                    distance = (140 + (100*numberOrbit) + r.Next(60, 120)); // Generate a moon distance from the star.

                    maxTrip = distance*distance*1000;
                    currentTrip = r.Next(0, maxTrip); // Set random angle

                    size = r.Next(1, 4); // Generate a scale

                    planetType = r.Next((int) PlanetTypes.Ice, (int) PlanetTypes.Barren); // Generate a planet type 

                    parentID = parent;

                    break;
            }
        }
    }

    public enum SpaceObjectType
    {
        Sun = 0,
        Planet = 1,
        Moon = 2
    }
}