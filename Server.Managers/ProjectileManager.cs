using System.Collections.Generic;
using System.Collections.Concurrent;
using Server.Models.Interfaces;
using Freecon.Core.Utils;

namespace Server.Managers
{
    public class ProjectileManager:IProjectileManager
    {
        public ConcurrentDictionary<int, Projectile> idToProjectile;

        
        public ProjectileManager()
        {
            idToProjectile = new ConcurrentDictionary<int, Projectile>();
        }

        /// <summary>
        /// Creates a projectile, returns projectileID
        /// If there is a projectileID collision, the projectile will simply be discarded
        /// </summary>
        /// <param name="ship"></param>
        /// <returns></returns>
        public int CreateProjectile(ICanFire ship, int projectileID)
        {            
            var tempProj = new Projectile(ship, projectileID);
            if (!idToProjectile.TryAdd(tempProj.ID, tempProj))
                ConsoleManager.WriteLine("Proj Collision " + tempProj.ID, ConsoleMessageType.Warning);
            //ConsoleManager.WriteToFreeLine("Projectiles: " + idToProjectile.Count.ToString());

            return projectileID;

            
        }                
        
        public void Update()
        {
            var idsToRemove = new List<int>();
            foreach (var kvp in idToProjectile)
            {
                if (TimeKeeper.MsSinceInitialization - kvp.Value.CreationTime > kvp.Value.Lifetime)
                    
                    idsToRemove.Add(kvp.Key); //Adds the ID for removal after this loop
            }

            for (int i = 0; i < idsToRemove.Count; i++)
            {
                RemoveProjectile(idsToRemove[i]);
            }
        }

        public void RemoveProjectile(int ID)
        {
            Projectile p;
            idToProjectile.TryRemove(ID, out p);
        }
      
    }

    public class Projectile
    {
        public int ID;
        public float CreationTime; //Milliseconds
        public ICanFire FiringObject = null; //ship that fired the projectile
        public double Lifetime;

        public Projectile(ICanFire firingObject, int projectileID)
        {
            ID = projectileID;
            FiringObject = firingObject;
            CreationTime = TimeKeeper.MsSinceInitialization;
            Lifetime = 20000;
        }
       
    }

    
}