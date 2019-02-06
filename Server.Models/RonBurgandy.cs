using System.Collections.Generic;

namespace Server.Models
{
    /// <summary>
    /// Used to report any news to a player.
    /// </summary>
    public class RonBurgundy
    {
        ICollection<string> _offlineNews { get; set; }

        public RonBurgundy()
        {
            _offlineNews = new List<string>();

        }

        /// <summary>
        /// Sends news to a player in the form of a chat message if he is online, otherwise stores for display upon login 
        /// </summary>
        public void SendNews()
        {


        }

    }
}
