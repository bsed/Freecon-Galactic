
using Freecon.Models.TypeEnums;
using Microsoft.Xna.Framework;
using Freecon.Client.Interfaces;
using System.Collections.Generic;
using Freecon.Client.Objects.Projectiles;
using Freecon.Models;

namespace Freecon.Client
{
    public class ProjectileRequest
    {
        public ICanFire FiringObj { get; set; }

        public IReadOnlyCollection<ProjectileData> ProjectileData { get { return _projectileData; } }

        List<ProjectileData> _projectileData;

        List<int> _projectileIDs;
        
        public bool SendToServer { get; set; }
        
        public byte FiringWeaponSlot { get; set; }

        public byte PctCharge { get; set; }        

        public ProjectileTypes ProjectileType { get; set; }
        public float FiringRotation { get; set; }


        /// <summary>
        /// ProjectileIDs must be unique
        /// </summary>
        /// <param name="firingObject"></param>
        /// <param name="firingRotation">rotation of fired weapon</param>
        /// <param name="firingWeaponSlot"></param>
        /// <param name="pctCharge"></param>
        /// <param name="sendToServer"></param>
        /// <param name="projectileType"></param>
        public ProjectileRequest(ICanFire firingObject, float firingRotation, byte firingWeaponSlot, byte pctCharge, bool sendToServer, ProjectileTypes projectileType)
        {
            _projectileData = new List<ProjectileData>();
            _projectileIDs = new List<int>();
            FiringObj = firingObject;
            SendToServer = sendToServer;
            FiringWeaponSlot = firingWeaponSlot;
            PctCharge = pctCharge;
            ProjectileType = projectileType;
            FiringRotation = firingRotation;
        }

        public void AddProjectile(ProjectileData d)
        {

            _projectileData.Add(d);
            _projectileIDs.Add(d.ProjectileID);
           
        }

        public List<int> GetProjectileIDs()
        {
            return _projectileIDs;
        }

    }

    public class ProjectileData
    {
        public Vector2 ProjectilePosition { get; set; }

        /// <summary>
        /// "Initial" velocity of the projectiles. Set this to holdingObj.LinearVelocity to offset projectilve velocity by ship velocity
        /// </summary>
        public Vector2 VelocityOffset { get; set; }
        public float ProjectileRotation { get; set; }
        public int ProjectileID { get; set; }
        public byte PctCharge { get; set; }
        
        public ProjectileData(   Vector2 projectilePosition, 
                                 Vector2 velocityOffset,
                                 float projectileRotation,
                                 int projectileID)
        {
            ProjectilePosition = projectilePosition;
            ProjectileID = projectileID;
            VelocityOffset = velocityOffset;
            ProjectileRotation = projectileRotation;
        }
    }

    
}
