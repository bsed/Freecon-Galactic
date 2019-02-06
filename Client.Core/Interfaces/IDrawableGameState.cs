using Freecon.Client.Mathematics;
using Freecon.Client.Core.States.Components;
using System.Collections.Generic;

namespace Freecon.Client.Core.Interfaces
{
    public interface IDrawableGameState: IGameState
    {
        Camera2D Camera { get; }

        void StateWillDraw(Camera2D camera);

        void StateDidDraw(Camera2D camera);

        IEnumerable<IDraw> DrawList { get; }

    }
}
