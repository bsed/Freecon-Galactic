using System;
using System.Collections.Generic;
using System.Linq;
using Freecon.Core.Networking.Models;
using Freecon.Core.Networking.Models.Messages;
using Freecon.Core.Utils;
using Server.Models;

namespace Server.Managers.OutgoingMessages
{
    public class ClientUpdateManager
    {

        PlayerManager _playerManager;

        public ClientUpdateManager(PlayerManager playerManager)
        {
            _playerManager = playerManager;
        }

        /// <summary>
        /// Forwards the data to online players as necessary. Does not send to the client corresponding to the given idToIgnore
        /// </summary>
        /// <param name="area"></param>
        /// <param name="data"></param>
        /// <param name="idToIgnore"></param>
        public void ForwardPositionUpdates(int? idToIgnore, IArea area, MessagePositionUpdateData data)
        {
            area.BroadcastMessage(new NetworkMessageContainer(data, MessageTypes.PositionUpdateData), idToIgnore,
                data.SendingPlayerID == null);
        }


        public void Update()
        {
            var players = _playerManager.GetAllObjects();
            players.Where(p => p.IsOnline).ForEach(p =>
            {
                //ListSendManager.SendHeartbeat();
                p.ClientTime += DateTime.Now - p.serverLastTime;
                p.serverLastTime = DateTime.Now;
            });
        }


        public void CheckClientTime(Player p, double time)
        {
            var ts = TimeSpan.FromMilliseconds(time);
            var t2 = p.ClientTime - ts;

            //Logger.log(Log_Type.INFO, "Server: " + p.ClientTime + ", Client: " + ts);
            //ConsoleManager.WriteToFreeLine("Server: " + p.ClientTime + ", Client: " + time + ", Diff: " + t2);

            if (p.HackCount != -1 || t2.Duration() <= new TimeSpan(0, 0, 1))
            {
                return;
            }

            p.HackCount++;
            p.ClientTime = ts;
        }
    }
}