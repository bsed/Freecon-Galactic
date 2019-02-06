using System;
using System.Collections.Generic;
using System.IO;

namespace Server.Utilities
{
    /// <summary>
    /// Provides random names from text files
    /// </summary>
    public class NameProvider
    {
        private List<string> ListOfNames;
        private Random r;

        public NameProvider(string filepath)
        {
            ListOfNames = new List<string>();
            string[] ParseMe = File.ReadAllLines(filepath);
            foreach (string s in ParseMe)
                ListOfNames.Add(s);
            r = new Random();
        }

        /// <summary>
        /// Grabs and removes a system name from list.
        /// </summary>
        /// <returns>A system name, in string format</returns>
        public string GetRandomName()
        {
            string returned = ListOfNames[r.Next(0, ListOfNames.Count - 1)]; // Random string
            return returned;
        }

        /// <summary>
        /// Grabs numEntries and combines them according to the delegate combiner
        /// </summary>
        /// <param name="numEntries"></param>
        /// <param name="combiner"></param>
        /// <returns></returns>
        public string GetRandomName(int numEntries, Func<List<string>, string> combiner)
        {
            //Just experimenting with a delegate argument.

            List<string> s = new List<string>(numEntries);
            for (int i = 0; i < numEntries; i++)
                s.Add(GetRandomName());

            return combiner(s);

        }

    }
}
