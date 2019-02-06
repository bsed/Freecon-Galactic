using Core.Models.Enums;
using Freecon.Models.TypeEnums;
using Server.Managers.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Freecon.Server.GameStates.GameStateStats
{
    public class KingOfTheColonyInitParams
    {
        public ServerGameStateTypes GameType { get { return ServerGameStateTypes.KingOfTheColony; } }        
    }
}
