using Freecon.Models;
using Freecon.Models.TypeEnums;

namespace Server.Models.Interfaces
{
    public interface ICollidable
    {
        bool DoCombatUpdates { get; set; }
        float TimeOfLastDamage { get; set; }
        int? CurrentAreaId { get; }
        float TimeOfLastCollision { get; set; }

        DebuffHandler Debuffs { get; }
        bool IsDead { get; set; }

        int Id { get; }

        /// <summary>
        /// Returns true if object is killed, false otherwise
        /// </summary>
        /// <param name="projectileType"></param>
        /// <param name="projectileID"></param>
        /// <param name="pctCharge"></param>
        /// <returns></returns>
        bool TakeDamage(ProjectileTypes projectileType, byte pctCharge, float multiplier);
        


    }
}
