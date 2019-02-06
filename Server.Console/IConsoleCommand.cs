using System;


namespace SRServer.Interfaces
{
    public interface IConsoleCommand
    {     

        /// <summary>
        /// Executes the command
        /// </summary>
        /// /// <param name="consoleString">Console string to be tested and parsed</param>
        /// <param name="executionData">Optional list of objects necessary for execution</param>
        bool TryExecute(String consoleString);

        void PrintHelp();

    }
}
