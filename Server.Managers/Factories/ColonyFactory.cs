using Server.Managers;
using Server.Models;
using Server.Models.Structures;
using SRServer.Services;

namespace Server.Factories
{
    public class ColonyFactory
    {
        static LocalIDManager _localIDManager;
        static GalaxyRegistrationManager _galaxyRegistrationManager;

        public static void Initialize(LocalIDManager idm, GalaxyRegistrationManager rm)
        {
            _localIDManager = idm;
            _galaxyRegistrationManager = rm;
        }

        /// <summary>
        /// Creates and registers a colony and all associated structures. Links colony to planet appropriately.
        /// </summary>
        /// <param name="xPos"></param>
        /// <param name="yPos"></param>
        /// <param name="owner"></param>
        /// <param name="area"></param>
        /// <param name="ls"></param>
        /// <returns></returns>
        public static Colony CreateColony(float xPos, float yPos, Player owner, Planet planet, LocatorService ls)
        {
            Colony c = new Colony(_localIDManager.PopFreeID(), owner, planet, ls);
            c.Name = "Colony " + planet.AreaName;
            c.AddStructure(StructureFactory.CreateCommandCenter(xPos, yPos, owner, planet.Id));
            Biodome b = StructureFactory.CreateBiodome(-9999999, -9999999, owner, planet.Id);
            c.AddStructure(b);
            b.Stats = new SmallBiodomeStats();
            b.Population = 10;


            _galaxyRegistrationManager.RegisterObject(c);

            planet.SetColony(c);
            planet.GetOwner().ColonizedPlanetIDs.Add(planet.Id);
            planet.IsColonized = true;
            planet.AddStructure(c.CommandCenter);
            planet.AddStructure(b);


            

            return c;

        }

    }
}
