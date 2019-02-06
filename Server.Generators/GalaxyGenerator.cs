using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Server.Managers;
using Freecon.Models.TypeEnums;
using SRServer.Services;
using Server.Models;
using Server.Models.Space;
using Server.Utilities;
using Core.Models;
using Core.Models.Enums;

namespace SRServer.Debug
{
    public class DebugGalaxyGenerator
    {
        // Seed random generator

        // Generation Variables
        private const int moonChance = 10; // Chance in % of moon
        private const int portChance = 100; // Chance in % of port
        private const int minDistance = 9; // Distance from sun
        private const int incrementOfOrbit = 120; // Space between orbits of planets
        private const int variationOfOrbit = 160; // Variation between orbits (Increment + Random(0, Variation))
        private const int chanceOfCluster = 20; // If all planets should custer together
        private static int ParadiseCount = 5; // Number of Paradise worlds
        private const int ParadiseChance = 1; // 1 - 100% chance that each check will pass
        private const int MaxAttempts = 2000; // Changes the number of attempts allowed
        private const int minimumPlanets = 5; // Minimum number of planets per system
        private const int maximumPlanets = 9; // Maximum number of planets per system

        // Moon
        private const int moonMinimumDistance = 206;
        private const int moonIncrementOfOrbit = 8;
        private const int moonVariationOfOrbit = 40;

        // Distance between objects
        private const int moonDistanceApart = 110; // Pixels apart of the moons
        private const int planetDistanceApart = 276; // Pixels apart of Planets
        private const int moonDistanceFromSun = 600; // Pixels from sun

        // Border
        private const int baseBorder = 500;
        private const int borderVariation = 500;
        private static readonly Random r = new Random(77657);
        public static Stack<int> DatabaseOfIDs = new Stack<int>();
        public static List<GenerationStar> DatabaseOfStars = new List<GenerationStar>();
        public static Dictionary<int, int> StarIDtoAreaID = new Dictionary<int, int>();

        IPlayerLocator _playerLocator;
        IAreaLocator _areaLocator;
        IShipLocator _shipLocator;

        static NameProvider NameProvider = new NameProvider(@"C:\SRDevGit\freecon-galactic\Star Names.txt");

        private static readonly int[] clusterRadii = new[]
                                                         {
                                                             // Levels of how close planets should be
                                                             600, // Tight cluster
                                                             800,
                                                             900,
                                                             950,
                                                             1000,
                                                             1100 // All on one side of system
                                                         };

        private static PSystem tempsys;
        private static Warphole tempWarp;


        public DebugGalaxyGenerator(IPlayerLocator pl, IAreaLocator al, IShipLocator sl)
        {
            _playerLocator = pl;
            _areaLocator = al;
            _shipLocator = sl;
        }


        public static PSystem GenerateStarsystem(int numberOfPlanets, int ID, GalaxyManager galaxyManager, LocalIDManager idManager, GalaxyRegistrationManager rm, LocatorService ls)
        {
            bool hasPort = false;

            // Add sun

            float size = r.Next(100, 255) / 100f;
            float mass = r.Next(50, 255) / 10f;
            float innerGravity = mass / 1875f;
            float outerGravity = innerGravity / 6f;

            var s = (SunTypes[])Enum.GetValues(typeof(SunTypes));
            int type = r.Next(0, s.Length);

            var star = new Star(size, mass, innerGravity, outerGravity, s[type]);    

            var system = new PSystem(star, ID, ls);
            system.AreaSize = baseBorder + r.Next(0, borderVariation);


            // Add Name
            system.AreaName = NameProvider.GetRandomName();

                    
            rm.RegisterObject(system);


            for (int i = 0; i < numberOfPlanets; i++)
            {
                Planet planet;
                bool SatisfiedWithResults = false;
                int numAttempts = MaxAttempts;
                system.IsCluster = false;
                int clusterType = 0;

                

                planet = new Planet(system, 0, ls);

                planet.AreaName = system.AreaName + " " + ToRoman(i + 1);

                if (r.Next(0, 101) > (100 - chanceOfCluster)) // Chance for planets to cluster
                {
                    system.IsCluster = true;
                    clusterType = r.Next(0, clusterRadii.Count()); // Get which type of cluster
                }

                planet.Init(i, moonChance, minDistance, incrementOfOrbit, variationOfOrbit, 0,
                                    r.Next());               
                SatisfiedWithResults = !CheckForCollisions(system, planet); // True if we find any collisions.



                while (!SatisfiedWithResults)// While we're not content with the generation
                {                                  

                    planet.Init(i, moonChance, minDistance, incrementOfOrbit, variationOfOrbit, 0,
                                    r.Next());                                 

                    SatisfiedWithResults = !CheckForCollisions(system, planet); // True if we find any collisions.

                    if (SatisfiedWithResults && system.IsCluster && i > 0)
                    {
                        SatisfiedWithResults = CheckForCollisions(system, planet, clusterRadii[clusterType]); // Cluster planets
                    }

                    numAttempts++;

                    if (numAttempts >= MaxAttempts && !system.IsCluster) 
                        break; // Breaks out of infinite pass if it ever occurs
                    else if (numAttempts >= MaxAttempts && system.IsCluster)
                    {
                        i = 0; // Reset the whole operation because there's a bad system being generated.
                        foreach (Planet pll in system.GetPlanets())
                        {
                            system.RemovePlanet(pll);
                            rm.DeRegisterObject(pll);
                        }
                        break;
                    }                                     

                } 

                if (SatisfiedWithResults == false) // Don't add a colliding planet!
                {
                    //Logger.log(Log_Type.WARNING, "Skipped adding a planet due to MaxAttempts being too much");
                    continue;
                }
                planet.Id = idManager.PopFreeID();
                system.AddPlanet(planet);
                planet.ParentAreaID = system.Id;
                rm.RegisterObject(planet);     
                
                if (system.AreaSize < planet.Distance - baseBorder) // If we're skinny, throw in a little extra space
                    system.AreaSize = planet.Distance + baseBorder + r.Next(0, borderVariation);

               
                             
            }


            foreach(Planet p in system.GetPlanets())
                _generateMoons(system, p, galaxyManager, idManager, rm, ls);

            foreach (Planet m in system.GetMoons())
            {
                if (system.AreaSize < ls.AreaLocator.GetArea(m.IDToOrbit).Distance + m.Distance + baseBorder)
                    // If we're skinny, throw in a little extra space
                    system.AreaSize = ls.AreaLocator.GetArea(m.IDToOrbit).Distance + m.Distance + baseBorder + r.Next(0, borderVariation);
            }

            // Port Generation
            if (r.Next(0, 100) > 100 - portChance && !hasPort)
            {
                if (system.MoonCount > 0)
                {
                    for (int i = 0; i < system.MoonCount; i++)
                    {
                    }
                    int cr = r.Next(0, system.MoonCount + 1); // Finds moon to turn into port.
                    int currentMoon = 0; // Used to get the moon

                    foreach (Planet m in system.GetMoons())
                    {
                        if (currentMoon == cr)
                        {
                            Planet moonToPort = m;
                            moonToPort.ParentAreaID = system.Id;
                            system.RemoveMoon(m);
                            rm.DeRegisterObject(m);

                            Port por = new Port(idManager.PopFreeID(), moonToPort, system.AreaName, ShipStatManager.StatShipList.ToList<ShipStats>(), ls); // Converts a moon into a port.
                            system.AddPort(por);
                            rm.RegisterObject(por);

                            hasPort = true;
                            break;
                        }
                        currentMoon++;
                    }
                }
            }


            return system;
        }

        public List<PSystem> generateAndFillGalaxy(int numPlanetsPerSystem, int solID, int numSystems, IEnumerable<PlanetLayout> layouts, GalaxyManager galaxyManager, LocalIDManager IDManager, GalaxyRegistrationManager rm, LocatorService ls)
        {
            float warpXPos = 0;
            float warpYPos = 0;
            int amount = 0;

            List<PSystem> generatedSystems = new List<PSystem>();

            PSystem sol = GenerateStarsystem(numPlanetsPerSystem, solID, galaxyManager, IDManager, rm, ls);
            sol.AreaName = "Sol";
            amount += sol.PlanetCount;
            generatedSystems.Add(sol);

            for (int i = 0; i < numSystems - 1; i++)
            {
                tempsys = GenerateStarsystem(numPlanetsPerSystem, IDManager.PopFreeID(),galaxyManager, IDManager, rm, ls);

                amount += tempsys.PlanetCount;
                generatedSystems.Add(tempsys);

                //GalaxyManager.idToSystem.TryAdd(tempsys.ID, tempsys);


            }

            
            //ConsoleManager.WriteToFreeLine("Average of " + amount / numSystems + " planets per system");

            //Randomly link the systems
            //Take each system, iterate through the systems list and randomly connect them, or don't
            for (int i = 0; i < generatedSystems.Count; i++)
                for (int j = i + 1; j < generatedSystems.Count; j++)
                {
                    if (r.Next(0, 100) <= 30) //30% probability of having the systems link
                    {
                        warpXPos = r.Next(-(generatedSystems[i].AreaSize - 200),
                                          (generatedSystems[i].AreaSize - 200));
                        warpYPos =-(int)Math.Sqrt(Math.Pow((generatedSystems[i].AreaSize - 200f), 2) - Math.Pow(warpXPos, 2));

                     

                        tempWarp = new Warphole(warpXPos/100, warpYPos/100, generatedSystems[i].Id, generatedSystems[j].Id,
                                                (byte)generatedSystems[i].Warpholes.Count);
                        tempWarp.DestinationAreaID = generatedSystems[j].Id;
                        generatedSystems[i].Warpholes.Add(tempWarp);

                        //Normalizing the vector
                        warpXPos = warpXPos / (generatedSystems[i].AreaSize - 200f);
                        warpYPos = warpYPos / (generatedSystems[i].AreaSize - 200f);

                        //Converting it to the length for the other system, flipping to other side of system
                        warpXPos = -warpXPos * (generatedSystems[j].AreaSize - 200f);
                        warpYPos = -warpYPos * (generatedSystems[j].AreaSize - 200f);
                        tempWarp = new Warphole(warpXPos/100, warpYPos/100, generatedSystems[j].Id, generatedSystems[i].Id,
                                                (byte)generatedSystems[j].Warpholes.Count);
                        tempWarp.DestinationAreaID = generatedSystems[i].Id;
                        generatedSystems[j].Warpholes.Add(tempWarp);
                    }
                }

            AssignPlanetLayouts(layouts, generatedSystems);

            return generatedSystems;
        }

        

        #region Collision Checking

        public static bool CheckForCollisions(PSystem p, Planet pp)
        {
            return CheckForCollisions(p, pp, planetDistanceApart);
        }

        public static bool CheckForCollisions(PSystem system, Planet pp, int PlanetApartDistanceToCheck)
        {
            if (system.PlanetCount >= 1)
            {
                float planet1PosX = 0, planet1PosY = 0;
                float planet2PosX = 0, planet2PosY = 0;
                float moon2PosX, moon2PosY;
                foreach (Planet p in system.GetPlanets())
                {
                    // Planet 1 Calculation
                    GetPosition(p, out planet1PosX, out planet1PosY);

                    // Planet 2 Calculation
                    GetPosition(pp, out planet2PosX, out planet2PosY);


                    // Distance check between points
                    float Distance = Distance2D(planet1PosX, planet1PosY, planet2PosX, planet2PosY);

                    if (Distance < PlanetApartDistanceToCheck) // Collision between planets
                    {
                        return true;
                    }
                }
                if (system.MoonCount <= 0) // Only continue if we've got moons, otherwise we're done.
                    return false;
                foreach (Planet m in system.GetMoons())
                {
                    if (m.IDToOrbit == pp.Id) // If we're checking parent, skip
                        continue;

                    foreach (Planet p in system.GetPlanets())
                    {
                        if (m.IDToOrbit != p.Id) // If we're checking parent, skip
                            continue;

                        // Planet 1 Calculation
                        GetPosition(p, out planet1PosX, out planet1PosY);

                        // Moon calculation
                        GetPosition(m, planet1PosX, planet1PosY, out moon2PosX, out moon2PosY);

                        // Distance check between points
                        float Distance;
                        Distance = Distance2D(planet2PosX, planet2PosY, moon2PosX, moon2PosY);

                        if (Distance < PlanetApartDistanceToCheck) // Collision between planets and moons
                        {
                            return true;
                        }
                    }
                }
            }
            return false; // Return false if no collisions
        }

        public static bool CheckForCollisions(PSystem p, Planet pp, Planet moon, GalaxyManager gm)
        {
            return CheckForCollisions(p, pp, moon, planetDistanceApart, moonDistanceFromSun, moonDistanceApart, gm);
        }

        public static bool CheckForCollisions(PSystem system, Planet pp, Planet moon, int planetDistanceApart,
                                              int moonDistanceFromSun, int moonDistanceApart, GalaxyManager galaxyManager)
        {
            // Generate locations of: Other planet, basePlanetPos+MoonPos, OtherMoonParentPos+MoonPos
            if (system.PlanetCount >= 1)
            {
                float moon1PosX = 0, moon1PosY = 0; // Outside because used twice
                float Distance;
                foreach (Planet p in system.GetPlanets())
                {
                    if (moon.IDToOrbit == p.Id)
                        continue;

                    // Planet 1 Calculation
                    float planet1PosX, planet1PosY;
                    GetPosition(p, out planet1PosX, out planet1PosY);


                    // Planet 2 Calculation
                    float planet2PosX, planet2PosY;
                    GetPosition(pp, out planet2PosX, out planet2PosY);


                    // Moon 1 Calculation
                    GetPosition(moon, planet2PosX, planet2PosY, out moon1PosX, out moon1PosY);


                    // Distance check between points
                    Distance = Distance2D(planet1PosX, planet1PosY, moon1PosX, moon1PosY);

                    if (Distance < planetDistanceApart) // We don't want moons too close to other planets!
                    {
                        return true;
                    }

                    // Distance check between points
                    Distance = Distance2D(0, 0, moon1PosX, moon1PosY); // Assuming sun is 0,0

                    if (Distance < moonDistanceFromSun) // We don't want moons too close to the sun!
                    {
                        return true;
                    }
                }

                // This is in a second forloop because otherwise we do waaaay extra calculations
                foreach (Planet m in system.GetMoons())
                {
                    // We need to get the parent planet to get the location
                    // Planet 1 Calculation
                    float planetParentPosX, planetParentPosY;
                    GetPosition((Planet)galaxyManager.GetArea(m.IDToOrbit), out planetParentPosX, out planetParentPosY);

                    // Moon 2 Calculation
                    float moon2PosX, moon2PosY;
                    GetPosition(m, planetParentPosX, planetParentPosY, out moon2PosX, out moon2PosY);

                    // Distance check between points
                    Distance = Distance2D(moon1PosX, moon1PosY, moon2PosX, moon2PosY);

                    if (Distance < moonDistanceApart) // We don't want moons too close to other moons!
                    {
                        return true;
                    }
                }
            }
            return false; // Return false if no collisions
        }

        #endregion


        /// <summary>
        /// Returns coordinates for the planet
        /// </summary>
        /// <param name="p">Planet to get info from</param>
        /// <param name="X">Out X Position</param>
        /// <param name="Y">Out Y Position</param>
        public static void GetPosition(Planet p, out float X, out float Y)
        {
            // 0-1f
            float increment = p.CurrentTrip / (float)p.MaxTrip;

            // Increment the angle
            double angle = (Math.PI * 2f) * increment;

            // Set positions
            X = (float)Math.Cos(angle) * p.Distance;
            Y = (float)Math.Sin(angle) * p.Distance;
        }

        /// <summary>
        /// Returns coordinates for the moon
        /// </summary>
        /// <param name="moon">Moon to get info from</param>
        /// <param name="parentX">Parent X Position</param>
        /// <param name="parentY">Parent Y Position</param>
        /// <param name="X">Out X Position</param>
        /// <param name="Y">Out Y Position</param>
        public static void GetPosition(Planet moon, float parentX, float parentY, out float X, out float Y)
        {
            // 0-1f
            float increment = moon.CurrentTrip / (float)moon.MaxTrip;

            // Increment the angle
            double angle = (Math.PI * 2f) * increment;

            // Set positions
            X = ((float)Math.Cos(angle) * moon.Distance) + parentX;
            Y = ((float)Math.Sin(angle) * moon.Distance) + parentY;
        }

        /// <summary>
        /// Finds the distance between two points on a 2D surface.
        /// </summary>
        /// <param name="x1">The point on the x-axis of the first point</param>
        /// <param name="x2">The point on the x-axis of the second point</param>
        /// <param name="y1">The point on the y-axis of the first point</param>
        /// <param name="y2">The point on the y-axis of the second point</param>
        /// <returns></returns>
        public static float Distance2D(float x1, float y1, float x2, float y2)
        {

            //Our end result
            float result = 0;
            //Take x2-x1, then square it
            double part1 = Math.Pow((x2 - x1), 2);
            //Take y2-y1, then sqaure it
            double part2 = Math.Pow((y2 - y1), 2);
            //Add both of the parts together
            double underRadical = part1 + part2;
            //Get the square root of the parts
            result = (float)Math.Sqrt(underRadical);
            //Return our result
            return result;
        }

        /// <summary>
        /// Trash code from StackOverflow to convert an Int to Roman Numerals.
        /// </summary>
        /// <param name="number">value greater than 0 because ancient people were stupid</param>
        /// <returns>String of roman numerals</returns>
        public static string ToRoman(int number)
        {
            if ((number < 0) || (number > 3999)) throw new ArgumentOutOfRangeException("insert value betwheen 1 and 3999");
            if (number < 1) return string.Empty;
            if (number >= 1000) return "M" + ToRoman(number - 1000);
            if (number >= 900) return "CM" + ToRoman(number - 900);
            if (number >= 500) return "D" + ToRoman(number - 500);
            if (number >= 400) return "CD" + ToRoman(number - 400);
            if (number >= 100) return "C" + ToRoman(number - 100);
            if (number >= 90) return "XC" + ToRoman(number - 90);
            if (number >= 50) return "L" + ToRoman(number - 50);
            if (number >= 40) return "XL" + ToRoman(number - 40);
            if (number >= 10) return "X" + ToRoman(number - 10);
            if (number >= 9) return "IX" + ToRoman(number - 9);
            if (number >= 5) return "V" + ToRoman(number - 5);
            if (number >= 4) return "IV" + ToRoman(number - 4);
            if (number >= 1) return "I" + ToRoman(number - 1);
            throw new ArgumentOutOfRangeException("something bad happened");//A+ exception description! 10/10 would throw again.
        }
       
        public static List<PlanetLayout> layouts;
        public static void AssignPlanetLayouts(IEnumerable<PlanetLayout> layouts, IEnumerable<PSystem> systems)
        {
            Random r = new Random(77756);
            foreach (PSystem s in systems)
            {
                foreach (Planet p in s.GetPlanets())
                {
                    p.Layout = layouts.ElementAt(r.Next(0, layouts.Count()));

                    foreach (Warphole w in p.Layout.Warpholes)
                    {
                        p.AddWarpholeAtLocation(w.PosX, w.PosY);


                    }
                }

                foreach (Planet p in s.GetMoons())
                {
                    p.Layout = layouts.ElementAt(r.Next(0, layouts.Count()));

                    foreach (Warphole w in p.Layout.Warpholes)
                    {
                        p.AddWarpholeAtLocation(w.PosX, w.PosY);


                    }
                }
            }
        }

        /// <summary>
        /// Generates n moons for a single planet, where n is divined from some mystical voodoo black magic related to planet.Scale
        /// </summary>
        /// <param name="system"></param>
        /// <param name="planet"></param>
        /// <param name="galaxyManager"></param>
        /// <param name="idManager"></param>
        /// <param name="rm"></param>
        /// <param name="pl"></param>
        /// <param name="al"></param>
        /// <param name="sl"></param>
        /// <param name="tl"></param>
        /// <param name="mem"></param>
        static void _generateMoons(PSystem system, Planet planet, GalaxyManager galaxyManager, LocalIDManager idManager, GalaxyRegistrationManager rm, LocatorService ls)
        {
            // If any moons, add them
            if (planet.HasMoons)
            {
                int parentID = planet.Id;

                int numMoons = 0;

                // Creates moon based on scale of size
                switch (planet.Scale)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                        planet.PlanetType = (PlanetTypes)r.Next(0, (int)PlanetTypes.OceanicSmall + 1);
                        planet.Scale += 3; // Makes the small sprites scale better
                        break;
                    case 6:
                    case 7:
                        planet.PlanetType = (PlanetTypes)r.Next(0, (int)PlanetTypes.OceanicSmall + 1);
                        if (planet.PlanetType >= PlanetTypes.Crystalline
                            && planet.PlanetType < PlanetTypes.Port)
                            planet.Scale += 3; // Makes the small sprites scale better
                        numMoons = r.Next(3) + 2;
                        break;
                    case 8:
                    case 9:
                        numMoons = r.Next(2) + 3;
                        break;
                    case 10:
                    case 11:
                    case 12:
                        numMoons = r.Next(3) + 3;
                        break;
                }

                for (int m = 0; m < numMoons; m++)
                {
                    Planet moon;
                    
                    moon = new Planet(planet, idManager.PopFreeID(), ls); 
                    InitializeMoon(moon, m, moonMinimumDistance, moonIncrementOfOrbit * m, moonVariationOfOrbit, planet, planet.Id,
                                  r.Next());
                    

                    if (moon.PlanetType >= PlanetTypes.Crystalline
                        && moon.PlanetType < PlanetTypes.Port)
                        moon.Scale += 3; // Makes the small sprites scale better

                    // #Yolo
                    moon.AreaName = planet.AreaName + "abcdefghijklmnopqrstuvwxyz"[m];

                    bool SatisfiedWithResults = false;
                    int maxAttempts = 200;
                    int numAttempts = 0;

                    while (!SatisfiedWithResults)// While we're not content with the generation
                    {
                        // Add moons
                        moon = new Planet(planet, moon.Id, ls);
                        InitializeMoon(moon, m, moonMinimumDistance, moonIncrementOfOrbit * m, moonVariationOfOrbit, planet, planet.Id,
                                      r.Next());
                        // Add another while here to check for maxDistance, as well as an If for if the system should follow that path of generation

                        SatisfiedWithResults = !CheckForCollisions(system, planet, moon, galaxyManager); // True if we find any collisions.
                        numAttempts++;

                        if (numAttempts >= maxAttempts)
                            break; // Breaks out of infinite pass if it ever occurs
                    } 

                    if (!SatisfiedWithResults) // Don't add a colliding planet!
                    {
                        //Logger.log(Log_Type.ERROR, "Skipped adding a moon due to MaxAttempts being too much");
                        idManager.PushFreeID(moon.Id);
                        
                        continue;
                    }

                    
                    moon.IsMoon = true;
                    rm.RegisterObject(moon);
                    system.AddMoon(moon);
                    moon.ParentAreaID = system.Id;
                }
            }


        }

        /// <summary>
        /// Creates a moon with a wide range of options.
        /// </summary>
        /// <param name="numberOrbit">Which orbit outwards (I, IV)</param>
        /// <param name="minDistance">Minimum distance, added to value of increment</param>
        /// <param name="incrementOfOrbit">How far apart the base distance between moons</param>
        /// <param name="variationOfOrbit">Fluctuation in distance between moons</param>
        /// <param name="parent">Planet to orbit</param>
        /// <param name="ID">ID to use when landing</param>
        /// <param name="randomSeed">Seed used for good randoms</param>
        static void InitializeMoon(Planet moon, int numberOrbit, int minDistance, int incrementOfOrbit, int variationOfOrbit,
                               Planet parent, int parentID, int randomSeed)
        {
            var r = new Random(randomSeed);


            moon.Distance = (minDistance + ((incrementOfOrbit + r.Next(0, variationOfOrbit)) * numberOrbit));
            // Generate a moon distance from the star.

            moon.MaxTrip = moon.Distance * moon.Distance * 1000;
            moon.CurrentTrip = r.Next(0, moon.MaxTrip); // Set random angle

            moon.Scale = (byte)r.Next(1, 4); // Generate a scale

            moon.PlanetType = (PlanetTypes)r.Next(0, (int)PlanetTypes.OceanicSmall + 1); // Generate a planet type 

            moon.IDToOrbit = parentID;

        }

        public static IEnumerable<PlanetLayout> ReadPlanetLayoutsFromDisk(string layoutDirectory)
        {
            layouts = new List<PlanetLayout>();

            string[] filePaths = Directory.GetFiles(layoutDirectory + @"/Layouts/", "*.txt");//+ @"\Layouts\", "*.txt");

            foreach (string path in filePaths)
            {
                layouts.Add(new PlanetLayout(path) { Id = layouts.Count });
            }

            return layouts;

        }
    }

    

}



