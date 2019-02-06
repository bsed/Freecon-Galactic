using Freecon.Client.Core;
using System;
using Freecon.Client.Managers;

namespace Freecon.Client.Xna.Windows
{
#if WINDOWS || XBOX
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main(string[] args)
        {

            //try
            //{

                using (var game = new WindowsMain(ClientEnvironmentType.Development))
                {
                    game.Run();
                }
            //}
            //catch(Exception e)
            //{
            //    Console.WriteLine(e.Message);
            //    Console.WriteLine(e.StackTrace);
            //    ClientLogger.LogError(e);
            //}

        }

    }
#endif
}