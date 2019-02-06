using System;

namespace Core
{
    public class CorruptStateException : Exception
    {
        public CorruptStateException(string message)
            : base(message)
        {

        }

    }
}
