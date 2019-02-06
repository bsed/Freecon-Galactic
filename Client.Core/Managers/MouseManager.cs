using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
namespace Freecon.Client.Managers
{
    /// <summary>
    /// A static class for getting mouse state.
    /// </summary>
    public class MouseManager
    {
        static MouseState _oldState, _currentState;

        public static Vector2 CurrentPosition { get { return _currentPosition; } }
        static Vector2 _currentPosition;

        public static Vector2 OldPosition { get { return _oldPosition; } }
        static Vector2 _oldPosition;

        public static bool MouseMoved { get { return _currentState.X != _oldState.X || _currentState.Y != _oldState.Y; } }

        public static bool LeftMouseClicked { get { return _currentState.LeftButton == ButtonState.Pressed && _oldState.LeftButton == ButtonState.Released; } }
        
        public static bool RightMouseClicked { get { return _currentState.RightButton == ButtonState.Pressed && _oldState.RightButton == ButtonState.Released; } }
        
        public static bool ScrolledUp { get { return _currentState.ScrollWheelValue > _oldState.ScrollWheelValue; } }
        
        public static bool ScrolledDown { get { return _currentState.ScrollWheelValue < _oldState.ScrollWheelValue; } }
        
        /// <summary>
        /// True while the button is down
        /// </summary>
        public static bool LeftButtonDown { get { return _currentState.LeftButton == ButtonState.Pressed; } }

        /// <summary>
        /// True while the button is down
        /// </summary>
        public static bool RightButtonDown { get { return _currentState.RightButton == ButtonState.Pressed; } }

        /// <summary>
        /// True if the button is down in the current frame and up in the last frame
        /// </summary>
        public static bool LeftButtonPressed { get { return _currentState.LeftButton == ButtonState.Pressed && _oldState.LeftButton == ButtonState.Released; } }

        /// <summary>
        /// True if the button is down in the current frame and up in the last frame
        /// </summary>
        public static bool RightButtonPressed { get { return _currentState.RightButton == ButtonState.Pressed && _oldState.RightButton == ButtonState.Released; } }

        /// <summary>
        /// True if button is up in current frame and was down in last frame
        /// </summary>
        public static bool LeftButtonReleased { get { return _currentState.LeftButton == ButtonState.Released && _oldState.LeftButton == ButtonState.Pressed; } }

        /// <summary>
        /// True if button is up in current frame and was down in last frame
        /// </summary>
        public static bool RightButtonReleased { get { return _currentState.RightButton == ButtonState.Released && _oldState.RightButton == ButtonState.Pressed; } }

        /// <summary>
        /// True if button is down in current frame and was down in last frame
        /// </summary>
        public static bool LeftButtonHeld { get { return _currentState.LeftButton == ButtonState.Pressed && _oldState.LeftButton == ButtonState.Pressed; } }

        /// <summary>
        /// True if button is down in current frame and was down in last frame
        /// </summary>
        public static bool RightButtonHeld { get { return _currentState.RightButton == ButtonState.Pressed && _oldState.RightButton == ButtonState.Pressed; } }

        public static void Update(bool Active)
        {
            if (Active) // If GameWindow isn't clicked into
            {
                _oldState = _currentState;
                _oldPosition = _currentPosition;

                _currentState = Mouse.GetState();
                _currentPosition = new Vector2(_currentState.X, _currentState.Y);
            }
        }
    }
}