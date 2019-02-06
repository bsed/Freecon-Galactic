using System.Collections.Generic;

namespace Freecon.Core.Networking.Models
{
    public class MessageAddRemoveShipsTeam : MessagePackSerializableObject
    {
        public int TeamID { get; set; }
        public List<int> IDs { get; set; }

        /// <summary>
        /// If true, add to team. If false, remove from team.
        /// </summary>
        public bool AddOrRemove { get; set; }

        public MessageAddRemoveShipsTeam()
        {
            IDs = new List<int>();
        }

    }
}
