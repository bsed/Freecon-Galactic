using Freecon.Client.Core.States.Components;
using Freecon.Client.Mathematics;
using Freecon.Client.Core.Interfaces;

namespace Freecon.Client.Core.Services
{
    public class CameraService
    {
        protected IGameState _activeState;

        public Camera2D ActiveCamera { get { return GetActiveCamera(); } }

        public float CurrentZoom { get { return GetCurrentZoom(); } }

        protected Camera2D GetActiveCamera()
        {
            var state = _activeState as IDrawableGameState;

            if (state != null)
            {
                return state.Camera;
            }

            return null;
        }

        protected float GetCurrentZoom()
        {
            var state = _activeState as IDrawableGameState;

            if (state != null)
            {
                return state.Camera.Zoom;
            }

            return 1f;
        }

        public void SetActiveState(IGameState active)
        {
            _activeState = active;
        }
    }
}
