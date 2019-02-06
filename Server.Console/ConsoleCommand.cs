using System;
using System.Collections.Generic;

namespace Server.Managers
{
    /// <summary>
    /// Shamelessly canabalized from the client. Should consolidate into a single reference eventually.
    /// </summary>
    public class CommandCallbackMap
    {

        Dictionary<string, Func<List<string>, object>> _argMap = new Dictionary<string, Func<List<string>, object>>();//Methods which take a string of space delimited arguments
        Dictionary<string, Action> _voidMap = new Dictionary<string, Action>(); //For methods with void return and no arguments

        HashSet<string> _mappedInstructions = new HashSet<string>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsCallingName">The instruction which will be passed as a string from CefSharp</param>
        /// <param name="callback">The method will be called</param>
        public void RegisterCommandCallback(string instruction, Func<object, List<string>> callback)
        {
            if (_mappedInstructions.Contains(instruction))
            {
                Console.WriteLine("Error, key already mapped");
                return;
            }
            _argMap.Add(instruction, callback);
            _mappedInstructions.Add(instruction);
        }

        public void RegisterCommandCallback(string instruction, Action callback)
        {
            if (_mappedInstructions.Contains(instruction))
            {
                Console.WriteLine("Error, key already mapped");
                return;
            }
            _voidMap.Add(instruction, callback);
            _mappedInstructions.Add(instruction);
        }

        public bool IsCommandValid(string instruction)
        {
            return _mappedInstructions.Contains(instruction);
        }

        /// <summary>
        /// Instruction format is a string beggining with the command, with space delimited arg list (e.g. "[COMMAND] [ARG1] [ARG2]... [ARGN]")
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public object TryParseAndExecute(string inputString)
        {
            List<string> ss = new List<string>(inputString.Split(' '));
            string command = ss[0];


            if (_voidMap.ContainsKey(command))
            {
                _voidMap[command]();
                return null;
            }
            else if (_argMap.ContainsKey(command))
            {
                if (ss.Count > 1)
                {
                    return _argMap[command](ss.GetRange(1, ss.Count - 1));
                }
                else
                    return _argMap[command](new List<string>{""});
            }
            else
            {
                Console.WriteLine("Error: instruction " + command + " not mapped.");
                return null;
            }

        }

    }

}




                    
                   
                   
                    

                   


                    
                    

                    
 
                

