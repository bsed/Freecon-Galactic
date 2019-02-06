using Freecon.Core.Networking.Models.ServerToServer;
using System.Collections.Generic;

namespace Freecon.Core.Networking.ServerToServer
{
    /// <summary>
    /// Sent when a team is dissolved, to players on servers other than the one handling the dissolution
    /// </summary>
    public class PlayerRemoveTeam : MessageServerToServer
    {
        public HashSet<int> PlayerIDs;
        public int TeamIDToRemove;

        public PlayerRemoveTeam()
        { }

        public PlayerRemoveTeam(HashSet<int> playerID, int teamIDToRemove) { PlayerIDs = playerID; TeamIDToRemove = teamIDToRemove; }
    }
}
