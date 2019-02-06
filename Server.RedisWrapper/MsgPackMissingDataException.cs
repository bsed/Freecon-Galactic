using System;

namespace RedisWrapper
{
    public class MsgPackMissingDataException : Exception
    {
        public MsgPackMissingDataException(string message) : base(message)
        {
            
        }
    }
}
