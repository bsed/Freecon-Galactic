using Freecon.Core.Utils;
using System;


namespace SRServer
{
    internal partial class Program
    {        
        private static void Main(string[] args)
        {
            ConsoleMover.SetConsolePosition(0, 340);
            Server s = new Server();
            
           

            s.Start();

            while (true)
                Console.ReadLine();//Keeps console app open indefinitely


        }

    }

}