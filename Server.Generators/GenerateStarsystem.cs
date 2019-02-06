//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Server.Managers;
//using System.IO;
//using Server.Models;
//using Server.Models.Space;


//namespace Mathematics
//{
//    public class StarsystemGenerator
//    {
//        // Seed random generator

//        // Generation Variables
//        private const int moonChance = 10; // Chance in % of moon
//        private const int portChance = 100; // Chance in % of port
//        private const int minDistance = 900; // Distance from sun
//        private const int incrementOfOrbit = 120; // Space between orbits of planets
//        private const int variationOfOrbit = 160; // Variation between orbits (Increment + Random(0, Variation))
//        private const int chanceOfCluster = 20; // If all planets should custer together
//        private static int ParadiseCount = 5; // Number of Paradise worlds
//        private const int ParadiseChance = 1; // 1 - 100% chance that each check will pass
//        private const int MaxAttempts = 2000; // Changes the number of attempts allowed
//        private const int minimumPlanets = 5; // Minimum number of planets per system
//        private const int maximumPlanets = 9; // Maximum number of planets per system

//        // Moon
//        private const int moonMinimumDistance = 206;
//        private const int moonIncrementOfOrbit = 8;
//        private const int moonVariationOfOrbit = 40;

//        // Distance between objects
//        private const int moonDistanceApart = 110; // Pixels apart of the moons
//        private const int planetDistanceApart = 276; // Pixels apart of Planets
//        private const int moonDistanceFromSun = 600; // Pixels from sun

//        // Border
//        private const int baseBorder = 880;
//        private const int borderVariation = 200;
//        private static readonly Random r = new Random(665456);
//        public static Stack<int> DatabaseOfIDs = new Stack<int>();
//        public static List<GenerationStar> DatabaseOfStars = new List<GenerationStar>();
//        public static Dictionary<int, int> StarIDtoAreaID = new Dictionary<int, int>();
        

//        private static readonly int[] clusterRadii = new[]
//                                                         {
//                                                             // Levels of how close planets should be
//                                                             600, // Tight cluster
//                                                             800,
//                                                             900,
//                                                             950,
//                                                             1000,
//                                                             1100 // All on one side of system
//                                                         };

//        private static PSystem tempsys;
//        private static Warphole tempWarp;

//        static Random rand = new Random(65463);

//        //public static PSystem GenerateStarsystem(GalaxyManager galaxyManager, GalaxyIDManager IDManager)
//        //{
//        //    bool hasPort = false;

//        //    var p = new PSystem();

//        //    int NumberOfPlanets = r.Next(minimumPlanets, maximumPlanets + 1);

//        //    // Add Name
//        //    p.SystemName = SystemNameGrabber.GetSystemName();

//        //    // Add sun
//        //    p.star = new Star(r.Next());

//        //    for (int i = 0; i < NumberOfPlanets; i++)
//        //    {
//        //        Planet pp;
//        //        bool SatisfiedWithResults = false;
//        //        int currentAttempts = MaxAttempts;
//        //        p.isCluster = false;
//        //        int clusterType = 0;

//        //        if (r.Next(0, 101) > (100 - chanceOfCluster)) // Chance for planets to cluster
//        //        {
//        //            p.isCluster = true;
//        //            clusterType = r.Next(0, clusterRadii.Count()); // Get which type of cluster
//        //        }

//        //        do // Iterate until we're not colliding.
//        //        {
//        //            // Add planets
//        //            pp = new Planet(p);
//        //            // Add another while here to check for maxDistance, as well as an If for if the system should follow that path of generation
//        //            GalaxyManager.GiveFreeID(pp);
//        //            pp.CreatePlanet(i, moonChance, minDistance, incrementOfOrbit, variationOfOrbit, p.star.ID,
//        //                            r.Next());

//        //            SatisfiedWithResults = !CheckForCollisions(p, pp); // True if we find any collisions.

//        //            if (SatisfiedWithResults && p.isCluster && i > 0)
//        //            {
//        //                SatisfiedWithResults = CheckForCollisions(p, pp, clusterRadii[clusterType]); // Cluster planets
//        //                Logger.log(Log_Type.INFO,
//        //                           "Current System: " + p.SystemName + ", orbit: " + i + ", pass: " +
//        //                           SatisfiedWithResults);
//        //            }

//        //            currentAttempts--;

//        //            if (currentAttempts <= 0 && !p.isCluster) break; // Breaks out of infinite pass if it ever occurs
//        //            else if (currentAttempts <= MaxAttempts * -1 && p.isCluster)
//        //            {
//        //                i = 0; // Reset the whole operation because there's a bad system being generated.
//        //                p.planets.Clear();
//        //                Logger.log(Log_Type.WARNING,
//        //                           "Resetting at attempt " + currentAttempts + ", cluster size of " +
//        //                           clusterRadii[clusterType]);
//        //                Console.WriteLine("Resetting");
//        //                break;
//        //            }
//        //        } while (!SatisfiedWithResults); // While we're not content with the generation


//        //        if (SatisfiedWithResults == false) // Don't add a colliding planet!
//        //        {
//        //            Logger.log(Log_Type.WARNING, "Skipped adding a planet due to MaxAttempts being too much");
//        //            continue;
//        //        }
//        //        p.planets.Add(pp);
//        //        pp.ParentArea = p;


//        //        int count = 0;
//        //        // If any moons, add them
//        //        if (pp.hasMoons)
//        //        {
//        //            int parentID = pp.Id;

//        //            int howManyMoons = 0;

//        //            // Creates moon based on scale of size
//        //            switch (pp.scale)
//        //            {
//        //                case 0:
//        //                case 1:
//        //                case 2:
//        //                case 3:
//        //                case 4:
//        //                case 5:
//        //                    pp.planetType = r.Next(0, (int)PlanetTypes.OceanicSmall + 1);
//        //                    pp.scale += 3; // Makes the small sprites scale better
//        //                    break;
//        //                case 6:
//        //                case 7:
//        //                    pp.planetType = r.Next(0, (int)PlanetTypes.OceanicSmall + 1);
//        //                    if (pp.planetType >= (int)PlanetTypes.Crystalline
//        //                        && pp.planetType < (int)PlanetTypes.Port)
//        //                        pp.scale += 3; // Makes the small sprites scale better
//        //                    howManyMoons = r.Next(3) + 2;
//        //                    break;
//        //                case 8:
//        //                case 9:
//        //                    howManyMoons = r.Next(2) + 3;
//        //                    break;
//        //                case 10:
//        //                case 11:
//        //                case 12:
//        //                    howManyMoons = r.Next(3) + 3;
//        //                    break;
//        //            }
//        //            //pp.scale *= (int)1.5f;

//        //            for (int m = 0; m < howManyMoons; m++)
//        //            {
//        //                Moon moon;

//        //                // Offset for IDs, else errors clientside in the future.
//        //                // If we get errors in the future, we will need to implement
//        //                // A Post generation ID set method. Unless we deprecate string userdata (probable)
//        //                moon = new Moon(m, moonMinimumDistance, moonIncrementOfOrbit * m, moonVariationOfOrbit, pp, pp.Id,
//        //                              (int)(p.moons.Count + m), r.Next());
//        //                if (moon.PlanetType >= PlanetTypes.Crystalline
//        //                    && moon.PlanetType < PlanetTypes.Port)
//        //                    moon.size += 3; // Makes the small sprites scale better

//        //                SatisfiedWithResults = false;
//        //                currentAttempts = 200;
//        //                do // Iterate until we're not colliding.
//        //                {
//        //                    // Add moons
//        //                    moon = new Moon(m, moonMinimumDistance, moonIncrementOfOrbit * m, moonVariationOfOrbit, pp,
//        //                                  pp.Id, (int)(p.moons.Count + m), r.Next());
//        //                    // Add another while here to check for maxDistance, as well as an If for if the system should follow that path of generation

//        //                    SatisfiedWithResults = !CheckForCollisions(p, pp, moon); // True if we find any collisions.
//        //                    currentAttempts--;

//        //                    if (currentAttempts <= 0) break; // Breaks out of infinite pass if it ever occurs
//        //                } while (!SatisfiedWithResults); // While we're not content with the generation

//        //                if (!SatisfiedWithResults) // Don't add a colliding planet!
//        //                {
//        //                    Logger.log(Log_Type.ERROR, "Skipped adding a moon due to MaxAttempts being too much");
//        //                    continue;
//        //                }

//        //                p.moons.Add(moon);
//        //                moon.ParentArea = p;
//        //                count++;
//        //            }
//        //        }

//        //        for (int v = 0; v < p.planets.Count; v++)
//        //        // Checks for distance to properly accomidate planets and moons
//        //        {
//        //            if (p.areaSize < pp.Distance - baseBorder) // If we're skinny, throw in a little extra space
//        //                p.areaSize = pp.Distance + baseBorder + r.Next(0, borderVariation);

//        //            if (!pp.hasMoons) // If no moons, we're done here
//        //                continue;

//        //            for (int mq = 0; mq < p.moons.Count; mq++)
//        //            // Check all moons (Planets need a reference to their children)
//        //            {
//        //                if (p.moons[mq].PlanetToOrbit != pp) // If you're not the moon we're looking for, continue.
//        //                    continue;

//        //                if (p.areaSize < pp.Distance + p.moons[mq].Distance + baseBorder)
//        //                    // If we're skinny, throw in a little extra space
//        //                    p.areaSize = pp.Distance + p.moons[mq].Distance + baseBorder + r.Next(0, borderVariation);
//        //            }
//        //        }
//        //    }

//        //    // Port Generation
//        //    if (r.Next(0, 100) > 100 - portChance && !hasPort)
//        //    {
//        //        if (p.moons.Count > 0)
//        //        {
//        //            for (int i = 0; i < p.moons.Count; i++)
//        //            {
//        //            }
//        //            int cr = r.Next(0, p.moons.Count + 1); // Finds moon to turn into port.
//        //            int currentMoon = 0; // Used to get the moon

//        //            for (int q = 0; q < p.moons.Count; q++)
//        //            {
//        //                if (currentMoon == cr)
//        //                {
//        //                    Moon moonToPort = p.moons[q];
//        //                    moonToPort.ParentArea = p;
//        //                    p.moons.RemoveAt(q);
//        //                    p.ports.Add(new Port(moonToPort)); // Converts a moon into a port.
//        //                    GalaxyManager.GiveFreeID(p.ports.Last());
//        //                    hasPort = true;
//        //                    break;
//        //                }
//        //                currentMoon++;
//        //            }
//        //        }
//        //    }


//        //    return p;
//        //}

//        //public static void generateAndFillGalaxy(int numSystems)
//        //{
//        //    float warpXPos = 0;
//        //    float warpYPos = 0;
//        //    int amount = 0;
//        //    for (int i = 0; i < numSystems; i++)
//        //    {
//        //        tempsys = GenerateStarsystem();
//        //        amount += tempsys.planets.Count();
//        //        for (int p = 0; p < tempsys.ports.Count; p++)
//        //        {
//        //            //tempsys.ports[p].type = (byte)AreaTypes.port;
//        //        }
//        //        GalaxyManager.Systems.Add(tempsys);

//        //        GalaxyManager.idToSystem.TryAdd(tempsys.Id, tempsys);
//        //    }

//        //    int ParadiseCount = 5;
//        //    int ParadiseChance = 1;
//        //    while (ParadiseCount > 0)
//        //    {
//        //        for (int i = 0; i < GalaxyManager.Systems.Count; i++)
//        //        {
//        //            if (ParadiseCount <= 0) // Break when we meet quota
//        //                break;
//        //            for (int pc = 0; pc < GalaxyManager.Systems[i].planets.Count; pc++)
//        //            {
//        //                if (GalaxyManager.Systems[i].planets[pc].planetType != (byte)PlanetTypes.Paradise)
//        //                {
//        //                    if (r.Next(0, 100) <= ParadiseChance)
//        //                    {
//        //                        if (ParadiseCount <= 0) // If this isn't here, we might get more than the amount
//        //                            break;
//        //                        GalaxyManager.Systems[i].planets[pc].planetType = (byte)PlanetTypes.Paradise;

//        //                        // Allow it to be just a bit bigger
//        //                        if (GalaxyManager.Systems[i].planets[pc].scale < 10)
//        //                            GalaxyManager.Systems[i].planets[pc].scale =
//        //                                r.Next(Math.Max(GalaxyManager.Systems[i].planets[pc].scale, 8), 11);
//        //                        ParadiseCount--; // Remove from Paradise list
//        //                    }
//        //                }
//        //            }
//        //            /* // No paradise moons
//        //            for (int mc = 0; mc < GalaxyManager.systems[i].moons.Count; mc++)
//        //            {
//        //                if (GalaxyManager.systems[i].moons[mc].planetType != (byte)PlanetTypes.Paradise)
//        //                {
//        //                    if (r.Next(0, 100) <= ParadiseChance)
//        //                    {
//        //                        if (ParadiseCount <= 0) // If this isn't here, we might get more than the amount
//        //                            break;
//        //                        GalaxyManager.systems[i].moons[mc].planetType = (byte)PlanetTypes.Paradise;
//        //                        ParadiseCount--;
//        //                        ConsoleManager.WriteToFreeLine("Moon " + GalaxyManager.systems[i].moons[mc].ID + ", System " + GalaxyManager.systems[i].SystemName);
//        //                    }
//        //                }
//        //            }*/
//        //        }
//        //    }
//        //    ConsoleManager.WriteToFreeLine("Average of " + amount / numSystems + " planets per system");

//        //    //Randomly link the systems
//        //    //Take each system, iterate through the systems list and randomly connect them, or don't
//        //    for (int i = 0; i < GalaxyManager.Systems.Count; i++)
//        //        for (int j = i + 1; j < GalaxyManager.Systems.Count; j++)
//        //        {
//        //            if (r.Next(0, 100) <= 30) //100% probability of having the systems link
//        //            {
//        //                warpXPos = r.Next(-(GalaxyManager.Systems[i].areaSize - 200),
//        //                                  (GalaxyManager.Systems[i].areaSize - 200));
//        //                warpYPos =
//        //                    -(int)
//        //                     Math.Sqrt(Math.Pow((GalaxyManager.Systems[i].areaSize - 200f), 2) - Math.Pow(warpXPos, 2));
//        //                tempWarp = new Warphole(warpXPos, warpYPos, GalaxyManager.Systems[i].Id, GalaxyManager.Systems[j].Id,
//        //                                        (byte)GalaxyManager.Systems[i].Warpholes.Count);
//        //                tempWarp.DestinationAreaID = GalaxyManager.Systems[j].Id;
//        //                GalaxyManager.Systems[i].Warpholes.Add(tempWarp);

//        //                //Normalizing the vector
//        //                warpXPos = warpXPos / (GalaxyManager.Systems[i].areaSize - 200f);
//        //                warpYPos = warpYPos / (GalaxyManager.Systems[i].areaSize - 200f);

//        //                //Converting it to the length for the other system, flipping to other side of system
//        //                warpXPos = -warpXPos * (GalaxyManager.Systems[j].areaSize - 200f);
//        //                warpYPos = -warpYPos * (GalaxyManager.Systems[j].areaSize - 200f);
//        //                tempWarp = new Warphole(warpXPos, warpYPos, GalaxyManager.Systems[j].Id, GalaxyManager.Systems[i].Id,
//        //                                        (byte)GalaxyManager.Systems[j].Warpholes.Count);
//        //                tempWarp.DestinationAreaID = GalaxyManager.Systems[i].Id;
//        //                GalaxyManager.Systems[j].Warpholes.Add(tempWarp);
//        //            }
//        //        }
//        //}
        
//        public static bool CheckForCollisions(PSystem p, Planet pp)
//        {
//            return CheckForCollisions(p, pp, planetDistanceApart);
//        }

//        public static bool CheckForCollisions(PSystem system, Planet pp, int PlanetApartDistanceToCheck)
//        {
//            if (system.PlanetCount >= 1)
//            {
//                float planet1PosX = 0, planet1PosY = 0;
//                float planet2PosX = 0, planet2PosY = 0;
//                float moon2PosX, moon2PosY;
//                foreach(Planet p in system.GetPlanets())
//                {
//                    // Planet 1 Calculation
//                    GetPosition(p, out planet1PosX, out planet1PosY);

//                    // Planet 2 Calculation
//                    GetPosition(pp, out planet2PosX, out planet2PosY);


//                    // Distance check between points
//                    float Distance = Distance2D(planet1PosX, planet1PosY, planet2PosX, planet2PosY);

//                    if (Distance < PlanetApartDistanceToCheck) // Collision between planets
//                    {
//                        return true;
//                    }
//                }
//                if (system.MoonCount <= 0) // Only continue if we've got moons, otherwise we're done.
//                    return false;
//                foreach(Planet m in system.GetMoons())
//                {
//                    if (m.IDToOrbit == pp.Id) // If we're checking parent, skip
//                        continue;

//                    foreach(Planet p in system.GetPlanets())
//                    {
//                        if (m.IDToOrbit != p.Id) // If we're checking parent, skip
//                            continue;

//                        // Planet 1 Calculation
//                        GetPosition(p, out planet1PosX, out planet1PosY);

//                        // Moon calculation
//                        GetPosition(m, planet1PosX, planet1PosY, out moon2PosX, out moon2PosY);

//                        // Distance check between points
//                        float Distance;
//                        Distance = Distance2D(planet2PosX, planet2PosY, moon2PosX, moon2PosY);

//                        if (Distance < PlanetApartDistanceToCheck) // Collision between planets and moons
//                        {
//                            return true;
//                        }
//                    }
//                }
//            }
//            return false; // Return false if no collisions
//        }

//        public static bool CheckForCollisions(PSystem p, Planet pp, Planet moon, GalaxyManager gm)
//        {
//            return CheckForCollisions(p, pp, moon, planetDistanceApart, moonDistanceFromSun, moonDistanceApart, gm);
//        }

//        public static bool CheckForCollisions(PSystem system, Planet pp, Planet moon, int planetDistanceApart,
//                                              int moonDistanceFromSun, int moonDistanceApart, GalaxyManager gm)
//        {
//            // Generate locations of: Other planet, basePlanetPos+MoonPos, OtherMoonParentPos+MoonPos
//            if (system.PlanetCount >= 1)
//            {
//                float moon1PosX = 0, moon1PosY = 0; // Outside because used twice
//                float Distance;
//                foreach (Planet p in system.GetPlanets())
//                {
//                    if (moon.IDToOrbit == p.Id)
//                        continue;

//                    // Planet 1 Calculation
//                    float planet1PosX, planet1PosY;
//                    GetPosition(p, out planet1PosX, out planet1PosY);


//                    // Planet 2 Calculation
//                    float planet2PosX, planet2PosY;
//                    GetPosition(pp, out planet2PosX, out planet2PosY);


//                    // Moon 1 Calculation
//                    GetPosition(moon, planet2PosX, planet2PosY, out moon1PosX, out moon1PosY);


//                    // Distance check between points
//                    Distance = Distance2D(planet1PosX, planet1PosY, moon1PosX, moon1PosY);

//                    if (Distance < planetDistanceApart) // We don't want moons too close to other planets!
//                    {
//                        Console.WriteLine("Colliding moon on planet, " + Distance);
//                        Console.WriteLine(", " + Distance);
//                        return true;
//                    }

//                    // Distance check between points
//                    Distance = Distance2D(0, 0, moon1PosX, moon1PosY); // Assuming sun is 0,0

//                    if (Distance < moonDistanceFromSun) // We don't want moons too close to the sun!
//                    {
//                        Console.WriteLine("Colliding moon on sun, " + Distance);
//                        return true;
//                    }
//                }

//                // This is in a second forloop because otherwise we do waaaay extra calculations
//                foreach (Planet m in system.GetMoons())
//                {
//                    // We need to get the parent planet to get the location
//                    // Planet 1 Calculation
//                    float planetParentPosX, planetParentPosY;
//                    GetPosition((Planet)gm.GetArea(m.IDToOrbit), out planetParentPosX, out planetParentPosY);

//                    // Moon 2 Calculation
//                    float moon2PosX, moon2PosY;
//                    GetPosition(m, planetParentPosX, planetParentPosY, out moon2PosX, out moon2PosY);

//                    // Distance check between points
//                    Distance = Distance2D(moon1PosX, moon1PosY, moon2PosX, moon2PosY);

//                    if (Distance < moonDistanceApart) // We don't want moons too close to other moons!
//                    {
//                        Console.WriteLine("Colliding moon on moon, " + Distance);
//                        return true;
//                    }
//                }
//            }
//            return false; // Return false if no collisions
//        }

//        /// <summary>
//        /// Returns coordinates for the planet
//        /// </summary>
//        /// <param name="p">Planet to get info from</param>
//        /// <param name="X">Out X Position</param>
//        /// <param name="Y">Out Y Position</param>
//        public static void GetPosition(Planet p, out float X, out float Y)
//        {
//            // 0-1f
//            float increment = p.CurrentTrip / (float)p.MaxTrip;

//            // Increment the angle
//            double angle = (Math.PI * 2f) * increment;

//            // Set positions
//            X = (float)Math.Cos(angle) * p.Distance;
//            Y = (float)Math.Sin(angle) * p.Distance;
//        }

//        /// <summary>
//        /// Returns coordinates for the moon
//        /// </summary>
//        /// <param name="mm">Moon to get info from</param>
//        /// <param name="parentX">Parent X Position</param>
//        /// <param name="parentY">Parent Y Position</param>
//        /// <param name="X">Out X Position</param>
//        /// <param name="Y">Out Y Position</param>
//        public static void GetPosition(Planet mm, float parentX, float parentY, out float X, out float Y)
//        {
//            // 0-1f
//            float increment = mm.CurrentTrip / (float)mm.MaxTrip;

//            // Increment the angle
//            double angle = (Math.PI * 2f) * increment;

//            // Set positions
//            X = ((float)Math.Cos(angle) * mm.Distance) + parentX;
//            Y = ((float)Math.Sin(angle) * mm.Distance) + parentY;
//        }

//        /// <summary>
//        /// Finds the distance between two points on a 2D surface.
//        /// </summary>
//        /// <param name="x1">The point on the x-axis of the first point</param>
//        /// <param name="x2">The point on the x-axis of the second point</param>
//        /// <param name="y1">The point on the y-axis of the first point</param>
//        /// <param name="y2">The point on the y-axis of the second point</param>
//        /// <returns></returns>
//        public static float Distance2D(float x1, float y1, float x2, float y2)
//        {

//            //Our end result
//            float result = 0;
//            //Take x2-x1, then square it
//            double part1 = Math.Pow((x2 - x1), 2);
//            //Take y2-y1, then sqaure it
//            double part2 = Math.Pow((y2 - y1), 2);
//            //Add both of the parts together
//            double underRadical = part1 + part2;
//            //Get the square root of the parts
//            result = (float)Math.Sqrt(underRadical);
//            //Return our result
//            return result;
//        }
        

//        public static List<PlanetLayout> layouts;

//        public static IEnumerable<PlanetLayout> ReadPlanetLayoutsFromDisk(string layoutDirectory)
//        {
//            layouts = new List<PlanetLayout>();

//            string[] filePaths = Directory.GetFiles(layoutDirectory + @"/Layouts/", "*.txt");//+ @"\Layouts\", "*.txt");

//            foreach (string path in filePaths)
//            {
//                layouts.Add(new PlanetLayout(path) {Id = layouts.Count});
//            }
                     
//            return layouts;

//        }
//        public static void AssignPlanetLayouts(IEnumerable<PlanetLayout> layouts, GalaxyManager gm)
//        {
//            foreach (PSystem system in gm.Systems)
//            {
//                foreach (Planet p in system.GetPlanets())
//                {
//                    p.Layout = layouts.ElementAt(rand.Next(0, layouts.Count()));

//                    foreach (Warphole w in p.Layout.Warpholes)
//                    {
//                        p.AddWarpholeAtLocation(w.PosX, w.PosY);

//                    }
//                }

//                foreach (Planet m in system.GetMoons())
//                {
//                    m.Layout = layouts.ElementAt(rand.Next(0, layouts.Count()));

//                    foreach (Warphole w in m.Layout.Warpholes)
//                    {
//                        m.AddWarpholeAtLocation(w.PosX, w.PosY);

//                    }
//                }
//            }
//        }
//    }
//}
