using System;

namespace Server.Database
{
    public class WrongAreaTypeException : Exception
    {
        public WrongAreaTypeException(string message) : base(message) { }
    }
}