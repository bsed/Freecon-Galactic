using Freecon.Models.TypeEnums;
using Freecon.Client.Interfaces;
using Freecon.Client.Objects.Structures;

namespace Freecon.Client.Extensions
{
    public static class TeamExtensions
    {
                
        //For turrets
        private static bool OnSameTeam(this Turret turret, ITeamable t)
        {
            if (t is Turret && ((Turret)t).TurretType == TurretTypes.Planet)
                return true;//Currently only allow allied turrets on planets. Before changing this, make sure turrets on the server are changed to send team information first.
            else if (t is Structure)
                return true;
            else
                return t.Teams.Overlaps(turret.Teams);
            

        }

        
        public static bool OnSameTeam(this ITeamable t1, ITeamable t2)
        {
            if (t1 == t2)
                return true;
            else if (t1 is Turret)
                return ((Turret)t1).OnSameTeam(t2);
            else if (t2 is Turret)
                return ((Turret)t2).OnSameTeam(t1);
            else
                return (t1.Teams.Overlaps(t2.Teams));//WARNING: Overlaps is an O(n) function. If projectile collisions are causing slowdown, consider revising team system

        }

       


    }
}
