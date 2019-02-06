using Freecon.Client.Core;
using Freecon.Client.Managers;
using System;
using System.Threading;

namespace Freecon.Client.Bot
{
#if WINDOWS || XBOX
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main(string[] args)
        {
#if DEBUG

            //Thread.Sleep(2000);//Set to allow multiple startup projects, gives slave 2 seconds to start up since all projects are launched simultaneously
#endif
            if(args.Length >= 2)
            { 
                Debugging.AutoLoginName = args[0];
                Debugging.AutoLoginPassword = args[1];
            }

            Debugging.IsBot = true;

            try
            {
                var g = new BotnetMain(ClientEnvironmentType.Development);
                g.Start();
                //using (var game = new BotnetMain(ClientEnvironmentType.Development))
                //{
                //    game.Run();
                //}
                while (true)
                {
                    Thread.Sleep(20000);
                }

            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }

        }

    }
#endif
}