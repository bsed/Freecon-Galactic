using System;
using System.Collections.Generic;
using System.Net;
using Freecon.Core.Networking.Models;

namespace Freecon.Core.Networking
{
    public class PlayerSession : MessagePackSerializableObject
    {
        public string ApiToken { get; set; }

        public string IPAddress { get; set; }

        public DateTime LoginTime { get; set; }

        public int PlayerId { get; set; }

        public string PlayerName { get; set; }

        /// <summary>
        /// Leveraged by the Auth layer to make decisions about a user's action.
        /// </summary>
        public List<string> Roles { get; set; }

        public int SlaveId { get; set; }

        public PlayerSession()
        {
            Roles = new List<string>();
        }

        public PlayerSession(int playerId, string playerName, string apiToken, int slaveId, DateTime loginTime, IPAddress ipAddress, List<string> roles)
        {
            PlayerId = playerId;
            PlayerName = playerName;
            ApiToken = apiToken;
            SlaveId = slaveId;
            LoginTime = loginTime;
            IPAddress = ipAddress.ToString();
            Roles = roles;
        }
    }
}
