using System.Collections.Generic;
using Freecon.Client.Core.Services;
using Freecon.Client.Core.States;
using Freecon.Client.Core.States.Components;
using Freecon.Client.GUI;
using Freecon.Client.Mathematics;
using Freecon.Client.View.CefSharp;

namespace Client.Bot
{
    public class BotnetGameStateManager : GameStateManager
    {
        public BotnetGameStateManager(CameraService cameraService, IEnumerable<IGameState> gameStates, TextDrawingService textDrawingService, GlobalGameWebLayer globalGameWebLayer) : base(cameraService, gameStates, textDrawingService, globalGameWebLayer)
        {
        }


        public override void Draw(Camera2D camera)
        {
            
        }
    }
}
