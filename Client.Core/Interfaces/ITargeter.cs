using System.Collections.Generic;


namespace Freecon.Client.Interfaces
{
    public interface ITargeter : ITeamable
    {

        Dictionary<int, ITargetable> PotentialTargets { get; set; }

        ITargetable CurrentTarget { get; set; }

        bool IsBodyValid { get; }

 
    }
}
