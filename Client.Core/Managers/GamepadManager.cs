using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Freecon.Client.Managers
{
    public class GamepadManager
    {
        /// <summary>
        /// A static class for getting gamepad (controller) state.
        /// </summary>
        /// 

        #region Ship Control

        public static ButtonBind ThrustUp = new ButtonBind(Buttons.LeftThumbstickUp);
        public static ButtonBind ThrustDown = new ButtonBind(Buttons.LeftThumbstickDown);
        public static ButtonBind TurnLeft = new ButtonBind(Buttons.RightThumbstickLeft);
        public static ButtonBind TurnRight = new ButtonBind(Buttons.RightThumbstickRight);
        public static ButtonBind Boost = new ButtonBind(Buttons.LeftStick);
        public static ButtonBind FireMissile = new ButtonBind(Buttons.RightShoulder);
        public static ButtonBind FirePrimary = new ButtonBind(Buttons.RightTrigger);
        public static ButtonBind FireSecondary = new ButtonBind(Buttons.LeftTrigger);
       

        #endregion

        public static ButtonBind EnterMode = new ButtonBind(Buttons.Y);
        public static ButtonBind HoldPlayerPosition = new ButtonBind(Buttons.X);
        public static ButtonBind ZoomIn = new ButtonBind(Buttons.DPadUp);
        public static ButtonBind ZoomOut = new ButtonBind(Buttons.DPadDown);


        public static GamePadState oldState, currentState; // Static Keystates to be referenced in the rest of code.

        /// <summary>
        /// Gets the current gamepad state for the rest of the program to reference.
        /// </summary>
        /// <param name="gameTime"></param>
        public static void SetCurrentButtons(bool Active)
        {
            if (Active) // If GameWindow isn't clicked into
                currentState = GamePad.GetState(PlayerIndex.One);
            
        }

        /// <summary>
        /// Needs to be set at the end of the applications run cycle. 
        /// Sets the current state to the old state for the next iteration.
        /// </summary>
        /// <param name="gameTime"></param>
        public static void SetOldButtons(bool Active)
        {
            if (Active) // If GameWindow isn't clicked into
                oldState = currentState;
        }



        /// <summary>
        /// Holds information about a button for gameplay use; Customizable
        /// </summary>
        /// 
        public static bool IsPressed(Buttons button)
        {
            return currentState.IsButtonDown(button);
        }

        public static bool IsTapped(Buttons button)
        {
            return currentState.IsButtonDown(button) && oldState.IsButtonUp(button);
        }

        public class ButtonBind
        {
            private readonly Buttons[] buttons;
            private readonly bool singleButtonBind;

            public ButtonBind(Buttons button)
            {
                singleButtonBind = true;
                buttons = new Buttons[1] { button };
            }

            public ButtonBind(Buttons[] button)
            {
                singleButtonBind = false;
                this.buttons = button;
            }

            public bool IsBindTapped()
            {
                if (singleButtonBind)
                {
                    return GamepadManager.IsTapped(buttons[0]);
                }
                else
                {
                    for (int i = 0; i < buttons.Length; i++)
                    {
                        if (GamepadManager.IsTapped(buttons[i]))
                            return true;
                    }
                }
                return false;
            }

            public bool IsBindPressed()
            {
                if (singleButtonBind)
                {
                    return GamepadManager.IsPressed(buttons[0]);
                }
                else
                {
                    for (int i = 0; i < buttons.Length; i++)
                    {
                        if (GamepadManager.IsPressed(buttons[i]))
                            return true;
                    }
                }
                return false;
            }

        }

    }
}