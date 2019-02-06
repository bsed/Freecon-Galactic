using Freecon.Models.TypeEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Freecon.Server.GameStates
{
    public class GameStateStats
    {
        public int MaxNumPlayers { get; set; }

        public int MinNumPlayers { get; set; }

        public float GameLength { get; set; }//Ms

        public float IdleTimeLength { get; set; }//Ms, time before auto start while waiting on players

        public float EndingTimeLength { get; set; }

        public ShipTypes DefaultShipType { get; set; }
    }
}
