using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;

namespace Freecon.Client.Managers
{
    /// <summary>
    /// A static class for getting keyboard state.
    /// </summary>
    public class KeyboardManager
    {
        // These are the defaults for the key map

        #region Ship Control

        public static KeyBind ThrustUp = new KeyBind(new[]{Keys.Up, Keys.W});
        public static KeyBind ThrustDown = new KeyBind(new[]{Keys.Down, Keys.NumPad5, Keys.S});
        public static KeyBind TurnLeft = new KeyBind(new[]{Keys.Left, Keys.A});
        public static KeyBind TurnRight = new KeyBind(new[]{Keys.Right, Keys.D});
        public static KeyBind ThrustLateralLeft = new KeyBind(new[] { Keys.Q, Keys.NumPad7 });
        public static KeyBind ThrustLateralRight = new KeyBind(new[] { Keys.E, Keys.NumPad9 });

        public static KeyBind Boost = new KeyBind(new[] {Keys.LeftShift, Keys.RightShift, Keys.F2});

        public static KeyBind FireWeapon1 = new KeyBind(new[] { Keys.D1 });
        public static KeyBind FireWeapon2 = new KeyBind(new[] { Keys.D2 });
        public static KeyBind FireWeapon3 = new KeyBind(new[] { Keys.D3 });
        public static KeyBind FireWeapon4 = new KeyBind(new[] { Keys.D4 });
        public static KeyBind FireWeapon5 = new KeyBind(new[] { Keys.D5 });
        public static KeyBind FireMissile = new KeyBind(Keys.F3);

        #endregion

        public static KeyBind EnterChatline = new KeyBind(Keys.Enter);
        public static KeyBind EnterMode = new KeyBind(new[] { Keys.End, Keys.F1 });
        public static KeyBind ColonizeMode = new KeyBind(Keys.C);//This is temporary
        public static KeyBind PlaceTurret = new KeyBind(Keys.T);//Also temporary
        public static KeyBind PlaceMine = new KeyBind(Keys.M);//Yep, you guessed it, temporary

        public static KeyBind OrderAttack = new KeyBind(Keys.A);
        public static KeyBind OrderHoldPosition = new KeyBind(Keys.H);
        public static KeyBind OrderStop = new KeyBind(Keys.S);
        public static KeyBind HoldPlayerPosition = new KeyBind(Keys.NumPad0);
        
        

        #region Debug Binds
#if ADMIN

        public static KeyBind H = new KeyBind(Keys.H);
        public static KeyBind G = new KeyBind(Keys.G);

        public static KeyBind ChangeShip = new KeyBind(Keys.F9);
        public static KeyBind ReloadUI = new KeyBind(Keys.F6);
        public static KeyBind SetPosTo0 = new KeyBind(Keys.F8);
        public static KeyBind AdminMoveRight = new KeyBind(Keys.NumPad6);
        public static KeyBind AdminMoveLeft = new KeyBind(Keys.NumPad4);
        public static KeyBind AdminMoveUp = new KeyBind(Keys.NumPad8);
        public static KeyBind AdminMoveDown = new KeyBind(Keys.NumPad5);

        public static KeyBind LeaveToPlanet = new KeyBind(Keys.P);
        public static KeyBind LeaveToSpace = new KeyBind(Keys.S);


        public static KeyBind SwitchMissile = new KeyBind(Keys.Z);
#endif
        #endregion

        public static KeyboardState oldState, currentState; // Static Keystates to be referenced in the rest of code.
        public static bool IsTyping;

        #region Custom Key Mapping

        public static bool CheckForKeyBind(string s)
        {
            try
            {
                string[] split = s.Split(' ');
                Keys[] keyList = GetKey(split);
                switch (split[1])
                {
                    case "ThrustUp":
                        ThrustUp = new KeyBind(keyList);
                        break;
                    case "ThrustDown":
                        ThrustDown = new KeyBind(keyList);
                        break;
                    case "TurnLeft":
                        TurnLeft = new KeyBind(keyList);
                        break;
                    case "TurnRight":
                        TurnRight = new KeyBind(keyList);
                        break;
                    case "Boost":
                        Boost = new KeyBind(keyList);
                        break;
                    case "FireMissile":
                        FireMissile = new KeyBind(keyList);
                        break;
                    case "EnterChatline":
                        EnterChatline = new KeyBind(keyList);
                        break;
                    case "EnterMode":
                        EnterMode = new KeyBind(keyList);
                        break;
                    default:
                        return false;
                }
                if (keyList.Length > 1)
                    Console.WriteLine("Bound " + split[1] + " to " + keyList.Length + " keys.");
                else
                    Console.WriteLine("Bound " + split[1] + " to " + keyList[0]);
            }
            catch
            {
                return false;
            }
            return true;
        }

        private static Keys[] GetKey(string[] s)
        {
            var keyList = new List<Keys>();
            int index = 3;
            string toTest = "";
            bool test = false;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i].Contains("=") && s[i].Length > 1)
                {
                    index = i;
                    string[] ss = s[i].Split('=');
                    if (ss.Count() > 1)
                    {
                        toTest = ss[1];
                        test = true;
                    }
                    //else
                    //    Console.WriteLine("Skipping");
                    break;
                }
            }
            //Console.WriteLine("Index: " + index);
            for (int i = index; i < s.Length; i++)
            {
                var keyToAdd = Keys.None;
                if (test)
                {
                    if (ParseKey(toTest, ref keyToAdd))
                    {
                        test = false;
                        i--;
                        //Console.WriteLine("Key: " + keyToAdd);
                    }
                    continue;
                }
                if (ParseKey(s[i], ref keyToAdd))
                {
                    //Console.WriteLine("Key: " + keyToAdd);
                    keyList.Add(keyToAdd);
                    continue;
                }
                //Console.WriteLine("Atempt: " + s[i]);
            }
            return keyList.ToArray();
        }

        public static bool ParseKey(string s, ref Keys k)
        {
            try
            {
                if (s[s.Length - 1] == ',') // We separate by commas.
                    s = s.TrimEnd(',');

                k = (Keys) Enum.Parse(typeof (Keys), s);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        /// <summary>
        /// Gets the current keyboard state for the rest of the program to reference.
        /// </summary>
        /// <param name="gameTime"></param>
        public static void SetCurrentKeys(bool Active)
        {
            if (Active) // If GameWindow isn't clicked into
                currentState = Keyboard.GetState();
        }

        /// <summary>
        /// Needs to be set at the end of the applications run cycle. 
        /// Sets the current state to the old state for the next iteration.
        /// </summary>
        /// <param name="gameTime"></param>
        public static void SetOldKeys(bool Active)
        {
            if (Active) // If GameWindow isn't clicked into
                oldState = currentState;
        }


        public static void GetTyping(ref string currentString, ref bool hasText)
        {
            #region Key Input

            string oldcmd = currentString;
            bool shift = (IsPressed(Keys.LeftShift) || IsPressed(Keys.RightShift));
            bool ctrl = (IsPressed(Keys.LeftControl) || IsPressed(Keys.RightControl));
            bool alt = (IsPressed(Keys.LeftAlt) || IsPressed(Keys.RightAlt));
            //Alphabet, Numbers, Space
            for (int i = 0; i < 26; i++)
            {
                if (IsTapped(Keys.A + i))
                    if (shift)
                    {
                        currentString += (char) ('A' + i);
                        hasText = true;
                    }
                    else
                    {
                        currentString += (char) ('a' + i);
                        hasText = true;
                    }

                if (i <= 9 && (IsTapped(Keys.D0 + i) || IsTapped(Keys.NumPad0 + i)))
                {
                    if (!shift)
                    {
                        currentString += (char) ('0' + i);
                        hasText = true;
                    }
                    else if (i == 1)
                    {
                        currentString += '!';
                        hasText = true;
                    }
                    else if (i == 2)
                    {
                        currentString += '@';
                        hasText = true;
                    }
                    else if (i == 3)
                    {
                        currentString += '#';
                        hasText = true;
                    }
                    else if (i == 4)
                    {
                        currentString += '$';
                        hasText = true;
                    }
                    else if (i == 5)
                    {
                        currentString += '%';
                        hasText = true;
                    }
                    else if (i == 6)
                    {
                        currentString += '^';
                        hasText = true;
                    }
                    else if (i == 7)
                    {
                        currentString += '&';
                        hasText = true;
                    }
                    else if (i == 8)
                    {
                        currentString += '*';
                        hasText = true;
                    }
                    else if (i == 9)
                    {
                        currentString += '(';
                        hasText = true;
                    }
                    else if (i == 0)
                    {
                        currentString += ')';
                        hasText = true;
                    }
                }
            }
            if (IsTapped(Keys.OemBackslash))
            {
                currentString += '\\';
                hasText = true;
            }
            if (IsTapped(Keys.Subtract) || IsTapped(Keys.OemMinus))
            {
                currentString += shift ? '_' : '-';
                hasText = true;
            }
            if (IsTapped(Keys.OemQuotes))
            {
                currentString += shift ? '"' : '\'';
                hasText = true;
            }
            if (IsTapped(Keys.OemTilde))
            {
                currentString += shift ? '~' : '`';
                hasText = true;
            }
            if (IsTapped(Keys.OemSemicolon))
            {
                currentString += shift ? ':' : ';';
                hasText = true;
            }
            if (IsTapped(Keys.OemOpenBrackets))
            {
                currentString += shift ? '{' : '[';
                hasText = true;
            }
            if (IsTapped(Keys.OemCloseBrackets))
            {
                currentString += shift ? '}' : ']';
                hasText = true;
            }
            if (IsTapped(Keys.OemQuestion))
            {
                currentString += shift ? '?' : '/';
                hasText = true;
            }
            if (IsTapped(Keys.OemPipe))
            {
                currentString += shift ? '|' : '\\';
                hasText = true;
            }
            if (IsTapped(Keys.OemPlus))
            {
                currentString += shift ? '+' : '=';
                hasText = true;
            }
            if (IsTapped(Keys.Divide))
            {
                currentString += '/';
                hasText = true;
            }
            if (IsTapped(Keys.Space))
            {
                currentString += ' ';
                hasText = true;
            }
            if (IsTapped(Keys.OemPeriod))
            {
                if (shift) currentString += '>';
                else currentString += '.';
                hasText = true;
            }
            if (IsTapped(Keys.OemComma))
            {
                if (shift) currentString += '<';
                else currentString += ',';
                hasText = true;
            }

            //Control Keys
            if (IsPressed(Keys.Back))
            {
                if (IsTapped(Keys.Back))
                    currentString = (currentString.Length != 0
                                         ? currentString.Substring(0, currentString.Length - 1)
                                         : "");
            }

            #endregion
        }

        public static bool IsPressed(Keys key)
        {
            return currentState.IsKeyDown(key);
        }

        public static bool IsTapped(Keys key)
        {
            return currentState.IsKeyDown(key) && oldState.IsKeyUp(key);
        }

        public static bool GetIfIgnoreKey(Keys key)
        {
            #region Check Key

            //bool shift = (IsPressed(Keys.LeftShift) || IsPressed(Keys.RightShift));
            //bool ctrl = (IsPressed(Keys.LeftControl) || IsPressed(Keys.RightControl));
            //bool alt = (IsPressed(Keys.LeftAlt) || IsPressed(Keys.RightAlt));
            //Alphabet, Numbers, Space
            for (int i = 0; i < 26; i++)
            {
                if (IsPressed(Keys.A + i))
                    return true;

                if (i <= 9 && (IsTapped(Keys.D0 + i) || IsTapped(Keys.NumPad0 + i)))
                {
                    return true;
                }
            }
            if (IsTapped(Keys.OemBackslash))
            {
                return true;
            }
            if (IsTapped(Keys.Subtract) || IsTapped(Keys.OemMinus))
            {
                return true;
            }
            if (IsTapped(Keys.OemQuotes))
            {
                return true;
            }
            if (IsTapped(Keys.OemTilde))
            {
                return true;
            }
            if (IsTapped(Keys.OemSemicolon))
            {
                return true;
            }
            if (IsTapped(Keys.OemOpenBrackets))
            {
                return true;
            }
            if (IsTapped(Keys.OemCloseBrackets))
            {
                return true;
            }
            if (IsTapped(Keys.OemQuestion))
            {
                return true;
            }
            if (IsTapped(Keys.OemPipe))
            {
                return true;
            }
            if (IsTapped(Keys.OemPlus))
            {
                return true;
            }
            if (IsTapped(Keys.Divide))
            {
                return true;
            }
            if (IsTapped(Keys.Space))
            {
                return true;
            }
            if (IsTapped(Keys.OemPeriod))
            {
                return true;
            }
            if (IsTapped(Keys.OemComma))
            {
                return true;
            }

            //Control Keys
            if (IsPressed(Keys.Back))
            {
                if (IsTapped(Keys.Back))
                    return true;
            }

            return false;

            #endregion
        }
    }

    /// <summary>
    /// Holds information about a key for gameplay use; Customizable
    /// </summary>
    public class KeyBind
    {
        private readonly Keys[] keys;
        private readonly bool singleKeyBind;

        public KeyBind(Keys key)
        {
            singleKeyBind = true;
            keys = new Keys[1] {key};
        }

        public KeyBind(Keys[] keys)
        {
            singleKeyBind = false;
            this.keys = keys;
        }

        /// <summary>
        /// Checks if key is held this frame.
        /// </summary>
        /// <returns>If a bind is tapped.</returns>
        public bool IsBindTapped()
        {
            if (singleKeyBind)
            {
                if (KeyboardManager.IsTyping)
                    if (KeyboardManager.GetIfIgnoreKey(keys[0]))
                        return false;
                return KeyboardManager.IsTapped(keys[0]);
            }
            else
            {
                for (int i = 0; i < keys.Length; i++)
                {
                    if (KeyboardManager.IsTyping)
                        if (KeyboardManager.GetIfIgnoreKey(keys[i]))
                            return false;
                    if (KeyboardManager.IsTapped(keys[i]))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if key is held.
        /// </summary>
        /// <returns>If bind is pressed</returns>
        public bool IsBindPressed()
        {
            if (singleKeyBind)
            {
                if (KeyboardManager.IsTyping)
                    if (KeyboardManager.GetIfIgnoreKey(keys[0]))
                        return false;
                return KeyboardManager.IsPressed(keys[0]);
            }
            else
            {
                for (int i = 0; i < keys.Length; i++)
                {
                    if (KeyboardManager.IsTyping)
                        if (KeyboardManager.GetIfIgnoreKey(keys[i]))
                            return false;
                    if (KeyboardManager.IsPressed(keys[i]))
                        return true;
                }
            }
            return false;
        }
    }
}