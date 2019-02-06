using System.IO;

namespace Core.Logging
{
    public class SimpleLogger
    {
        StreamWriter s;
 
        public SimpleLogger(string filepath, bool append = true)
        {
            s = new StreamWriter(filepath, append);
        }

        public void Log(string text)
        {
            s.Write(text);
        }

        
    }
}
