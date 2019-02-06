using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Freecon.Client.Managers.Space;
using Freecon.Models.TypeEnums;
using Core.Models.Enums;
using Freecon.Client.Core.Objects;

namespace Freecon.Client.Managers
{
    public class SpaceManager
    {
        private readonly BorderManager _borderManager;
        private readonly GravityManager _gravityManager;

        /// <summary>
        /// Constructor for all planets within a list.
        /// </summary>
        // Seed random generator
        private readonly Random r;

        private readonly SpaceObjectManager _spaceObjectManager;

        private PhysicsManager _physicsManager;

        private WarpHoleManager _warpholeManager;

        private GameStates State = GameStates.updating;

        // Area for declaring variables pertaining to the list of planets.

        /// <summary>
        /// Creates system with Planets and Borders
        /// </summary>
        public SpaceManager(SpriteBatch spriteBatch, 
                            BorderManager borderManager, 
                            GravityManager gravityManager,
                            PhysicsManager physicsManager, 
                            SpaceObjectManager spaceObjectManager,
                            WarpHoleManager warpholeManager)
        {
            r = new Random(45546);
            _physicsManager = physicsManager;
            _warpholeManager = warpholeManager;

            _spaceObjectManager = spaceObjectManager;
            _borderManager = borderManager;
            _gravityManager = gravityManager;
        }

        public void Reset()
        {
            _spaceObjectManager.Reset();
            _borderManager.Reset(_spaceObjectManager.SizeOfSystem);
            _gravityManager.Reset(_spaceObjectManager.planetList);
        }

        public GameStates getState()
        {
            return State;
        }

        public virtual void Draw(SpriteBatch spriteBatch, Vector2 CamSpot, float zoom)
        {
            _spaceObjectManager.Draw(CamSpot, zoom);
            _borderManager.Draw(CamSpot, zoom);
        }

        public void CreatePlanet(int distance, int maxTrip, PlanetTypes type, float currentTrip, float scale, int ID,
                                 int parentID, bool isMoon)
        {
            _spaceObjectManager.CreateSinglePlanet(distance, maxTrip, type, currentTrip, scale, ID, parentID, isMoon);
        }

        public void CreatePort(int distance, int maxTrip, PlanetTypes type, float currentTrip, float scale, int ID,
                               int parentID, bool isMoon)
        {
            _spaceObjectManager.CreatePort(distance, maxTrip, type, currentTrip, scale, ID, parentID, isMoon);
        }

        public void CreateWarphole(float xpos, float ypos, byte warpIndex, int destinationAreaID)
        {
            _warpholeManager.CreateWarphole(xpos, ypos, warpIndex, destinationAreaID);
        }

        public void CreateBorderAndSunGravity(int sizeOfSystem)
        {
            _borderManager.UpdateBorder(sizeOfSystem);
            var sun = _spaceObjectManager.Sun;
            _gravityManager.CreateSunGravity(_spaceObjectManager.Sun.Body, sizeOfSystem, sun.InnerGravityStrength, sun.OuterGravityStrength);
        }

        public Sun InitializeSun(float radius, float density, float innerGravityStrength, float outerGravityStrength, SunTypes type)
        {
            return _spaceObjectManager.InitializeSun(radius, density, innerGravityStrength, outerGravityStrength, type);
        }


    }
}