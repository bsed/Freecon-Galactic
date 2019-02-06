using System.Collections.Generic;

namespace Freecon.Client.Interfaces
{
    public interface ITeamable
    {
        HashSet<int> Teams { get; set; }

        int Id { get; }
    }
}