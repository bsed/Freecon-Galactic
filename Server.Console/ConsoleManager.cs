using System;

namespace Server.Managers
{
    public class ConsoleManager
    {
        public static string format = "MMM d HH:mm:ss: ";

        private static string _inputString;

        public ConsoleManager()
        {

            _inputString = "";

            int numDebugLines = 40;

        }

        static ConsoleManager()
        {
            Console.BufferHeight = Int16.MaxValue-1;
        }

        public static void WriteLine(string toWrite)
        {
            Console.WriteLine(DateTime.Now.ToString(format) + toWrite);
        }

        public static void WriteLine(string toWrite, ConsoleMessageType messageType)
        {

            #region Color Switch

            ConsoleColor colorToUse = ConsoleColor.DarkGreen;
            switch (messageType)
            {
                case ConsoleMessageType.Error:
                    colorToUse = ConsoleColor.Red;
                    break;
                case ConsoleMessageType.Login:
                    colorToUse = ConsoleColor.DarkMagenta;
                    break;
                case ConsoleMessageType.Warning:
                    colorToUse = ConsoleColor.Yellow;
                    break;

                case ConsoleMessageType.Notification:
                    colorToUse = ConsoleColor.Cyan;
                    break;

                case ConsoleMessageType.ManagerRegistration:
                    colorToUse = ConsoleColor.Blue;
                    break;

                case ConsoleMessageType.NetworkMessage:
                    colorToUse = ConsoleColor.Magenta;
                    break;

                case ConsoleMessageType.Debug:
                    colorToUse = ConsoleColor.Gray;
                    break;
                case ConsoleMessageType.Startup:
                    colorToUse = ConsoleColor.Green;
                    break;

            }

            #endregion


                Console.ForegroundColor = colorToUse;
                Console.WriteLine(DateTime.Now.ToString(format) + toWrite);
                Console.ForegroundColor = ConsoleColor.White;

        }

        public static void WriteLine(Exception e)
        {
            WriteLine(e.Message, ConsoleMessageType.Error);
            WriteLine(e.StackTrace, ConsoleMessageType.Error);
        }
        
    }

    public enum ConsoleMessageType
    {
        Startup,
        Error,
        Warning,
        Notification,
        ManagerRegistration,
        NetworkMessage,
        Debug,
        Login,

    }
}