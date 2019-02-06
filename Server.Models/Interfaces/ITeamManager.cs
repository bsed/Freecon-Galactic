using System.Threading.Tasks;
namespace Server.Models
{
    public interface ITeamManager
    {
        Task<bool> AddPlayerToTeamAsync(Player p, int teamID);

        int CreateNewTeam(Player p);
    }

}
