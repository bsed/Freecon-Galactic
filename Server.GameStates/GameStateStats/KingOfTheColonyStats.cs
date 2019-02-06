using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Freecon.Server.GameStates
{
    public class KingOfTheColonyStats : GameStateStats
    {
        public KingOfTheColonyStats()
        {
            MaxNumPlayers = 10;
            MinNumPlayers = 2;
            GameLength = 5 * 60 * 1000;
            IdleTimeLength = 1 * 60 * 1000;
            EndingTimeLength = 1 * 60 * 1000;

        }
    }
}
