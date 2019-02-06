using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Models.Enums;


namespace Freecon.Server.GameStates
{
    public interface IServerGameStateModel
    {
        ServerGameStateTypes GameStateType { get; }

        int Id { get; }

    }
}
