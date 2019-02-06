using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Freecon.Client.Mathematics;
using Freecon.Models.TypeEnums;
#if ADMIN

#endif

namespace Freecon.Client.Managers
{
    public class ChatManager
    {
        public struct Chatline
        {
            public ChatTypes chatType;
            public string prefix;
            public Color prefixColor;
            public int row;
            public string text;
        }

        private int increment;

        public bool isMouseOverChatBox = false;

        public bool renderChat = true;

        private String currentString = "";

        private bool hasText;

        //Viewing the chatlist
        public Camera2D chatCamera;

        /// <summary>
        /// Commands in the console are parsed here.
        /// </summary>
        /// <param name="c">Chatline to read.</param>
        /// <returns>Returns parsed line, this is set to chat or sent to server after.</returns>
        public Chatline CheckCommands(Chatline c)
        {
            if (!c.text.StartsWith(@"/")) // Return if we're just radio-ing
#if ADMIN
            {
                if (!c.text.StartsWith(@".")) // Check for an admin command or not
                    return c;
                else
                {
                    c = CheckAdminCommands(c);
                    return c;
                }
            }
#else
            return c;
#endif

            c.text = c.text.Remove(0, 1); // Removes '/'
            if (c.text.Length == 0)
                return c;

            if (c.text.Length <=2)//Prevents client crashes from trying to remove from "/s "
            {
                c.chatType = ChatTypes.Error;
                c.text = "Invalid command or not enough arguments passed";
                c.prefix = "Error: ";
                return c;
            }

            string[] parsed = c.text.Split(' '); // Grab before the space
            switch (parsed[0].ToLower()) // Non-case sensitive commands
            {
                case "hug":
                    if (c.text.StartsWith("hug "))
                        c.text = c.text.Remove(0, 4);
                    else
                        c.text = c.text.Remove(0, 2);
                    c.chatType = ChatTypes.hug;
                    c.prefix = "";
                    break;

                case "online":
                    c.chatType = ChatTypes.whosonline;
                    c.prefix = "";
                    // Ask for players online
                    break;

                case "bloom":
                    c.chatType = ChatTypes.nodisplay;
                    var b = new Chatline();
                    b.prefixColor = Color.DarkCyan;
                    b.text = " ";
                    c.text = null;
                    if (SettingsManager.isBloom)
                    {
                        SettingsManager.isBloom = false;
                        b.prefix = "Bloom disabled.";
                    }
                    else
                    {
                        SettingsManager.isBloom = true;
                        b.prefix = "Bloom enabled.";
                    }
                    // DisplayToChatbox(b);
                    break;

                default:
                    c.chatType = ChatTypes.Error;
                    c.text = "Invalid command.";
                    c.prefix = "Error: ";
                    break;
            }

            return c;
        }

#if ADMIN
        /// <summary>
        /// Runs any admin commands specified
        /// </summary>
        /// <param name="c">Chatline read</param>
        /// <returns>Chatline returned, set to not display</returns>
        public Chatline CheckAdminCommands(Chatline c)
        {
            c.text = c.text.Remove(0, 1);//Remove slash
            if (c.text.Length <= 0)
                return c;
            string[] parsed = c.text.Split(' ');
            switch (parsed[0].ToLower())
            {
                case "system":
                //case "systemstats":
                //    _messageManager.sendAdminTextInput("", _clientManager.Client, (byte) AdminCommands.systemStatistics);
                    break;
                //case "setship":
                //    _messageManager.sendAdminTextInput(parsed[1], _clientManager.Client, (byte)AdminCommands.setShip);
                //    break;
                case "who":
                case "whoplayer":
                    break;
                case "killplayer":
                case "kill":
                    break;

                //case "ally":
                //    _messageManager.sendAdminTextInput(_clientShipManager.PlayerShip.Teams.ElementAt(0).ToString(), _clientManager.Client, (byte)AdminCommands.allyNPCs);
                    //break;
                //case "makenpc":                    
                //    _messageManager.sendAdminTextInput(parsed[1], _clientManager.Client, (byte)AdminCommands.makeNPCs);
                //    break;
                //case "stop":
                    //_clientShipManager.PlayerShip._body.LinearVelocity = Vector2.Zero;
                    //break;

                    
            }
            c.chatType = ChatTypes.nodisplay;
            return c;
        }
#endif

        /// <summary>
        /// Returns if message can be split
        /// </summary>
        /// <param name="chatType">ChatType to check</param>
        /// <returns>If Splitable</returns>
        private bool IsSplitable(ChatTypes chatType)
        {
            // If a player wrote the message, our line-split character should be included in the message
            switch (chatType)
            {
                case ChatTypes.Radio:
                case ChatTypes.Shout:
                case ChatTypes.tell:
                case ChatTypes.hug:
                case ChatTypes.admins:
                    return false;
                default:
                    return true;
            }
        }

        /// <summary>
        /// Returns a color for the prefix, based on the chat type.
        /// </summary>
        /// <param name="chatType">Chat Type Enum</param>
        /// <returns>Color to Draw</returns>
        private Color GetPrefixColor(ChatTypes chatType)
        {
            //Console.WriteLine("Value " + chatType);
            switch (chatType)
            {
                case ChatTypes.Radio:
                    return Color.MediumPurple;

                case ChatTypes.tell:
                    return Color.LightBlue;

                case ChatTypes.hug:
                    return Color.MediumSpringGreen;

                case ChatTypes.Shout:
                    return Color.LimeGreen;

                case ChatTypes.admin:
                    return Color.HotPink;

                case ChatTypes.admins:
                    return Color.HotPink;

                case ChatTypes.help:
                    return Color.Yellow;

                case ChatTypes.warp:
                    return Color.Turquoise;

                case ChatTypes.whosonline:
                    return Color.SteelBlue;

                case ChatTypes.login:
                    return Color.MediumTurquoise;

                case ChatTypes.Alert:
                    return Color.OrangeRed;

                default:
                    return Color.White;
            }
        }

        private bool IsPressed(Keys key)
        {
            return KeyboardManager.currentState.IsKeyDown(key);
        }

        private bool IsTapped(Keys key)
        {
            return KeyboardManager.currentState.IsKeyDown(key) && KeyboardManager.oldState.IsKeyUp(key);
        }
    }
}

