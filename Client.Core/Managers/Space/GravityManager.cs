using System.Collections.Generic;
using System.Linq;
using FarseerPhysics.Controllers;
using FarseerPhysics.Dynamics;
using Freecon.Client.Objects;

namespace Freecon.Client.Managers.Space
{
    public class GravityManager
    {
        private GravityController planetGravity;

        private GravityController innerGravity, outerGravity;

        private PhysicsManager _physicsManager;

        public GravityManager(PhysicsManager physicsManager)
        {
            _physicsManager = physicsManager;
        }

        public void Reset(List<Planet> planetList)
        {
            // Free's branch values are (.28f, 600, 0)
            planetGravity = new GravityController(.2f, 400, 0);
            _physicsManager.World.AddController(planetGravity);

            for (int i = 0; i < planetList.Count(); i++)
            {
                planetGravity.AddBody(planetList[i].body);
            }
        }

        public void CreateSunGravity(Body sun, int SizeOfSystem, float innerStrength, float outerStrength)
        {
            //Not sure why, but suddenly this crashes on warps without the .Contains check
            if (innerGravity != null && _physicsManager.World.ControllerList.Contains(innerGravity))
            {
                _physicsManager.World.RemoveController(innerGravity);
            }

            if (outerGravity != null && _physicsManager.World.ControllerList.Contains(outerGravity))
            {
                _physicsManager.World.RemoveController(outerGravity);
            }

            // Free's Branch: (0.004f, SizeOfSystem / 5, 0)
            innerGravity = new GravityController(innerStrength, SizeOfSystem/5, 0);
                
            innerGravity.AddBody(sun);

            // Free's Branch: (0.00018f, SizeOfSystem + 1000, SizeOfSystem / 10)
            outerGravity = new GravityController(outerStrength, SizeOfSystem + 1000, SizeOfSystem/10);
                
            outerGravity.AddBody(sun);

            _physicsManager.World.AddController(innerGravity);
            _physicsManager.World.AddController(outerGravity);
        }
    }
}