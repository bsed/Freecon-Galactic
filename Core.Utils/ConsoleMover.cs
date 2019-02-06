using System;
using System.Runtime.InteropServices;

namespace Freecon.Core.Utils
{

    public class ConsoleMover
    {
        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();

        private static IntPtr MyConsole = GetConsoleWindow();

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);


        const int SWP_NOSIZE = 0x0001;
        
        /// <summary>
        /// Sets the console position relative to the top left corner of the main screen.
        /// </summary>
        /// <param name="xPos"></param>
        /// <param name="yPos"></param>
        public static void SetConsolePosition(int xPos, int yPos)
        {
            SetWindowPos(MyConsole, 0, xPos, yPos, 0, 0, SWP_NOSIZE);


        }
            
            
    }
}
