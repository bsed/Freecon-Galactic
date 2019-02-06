using System.Collections.Generic;

namespace Server.Models.Interfaces
{
    public interface ITeamable
    {
        HashSet<int> GetTeamIDs();

    }
}
