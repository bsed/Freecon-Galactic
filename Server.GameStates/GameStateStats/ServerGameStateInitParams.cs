using Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Freecon.Server.GameStates.GameStateStats
{
    public abstract class ServerGameStateInitParams
    {
        public abstract ServerGameStateTypes GameType {get;}
    }
}
