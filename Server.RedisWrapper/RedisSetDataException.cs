using System;

namespace RedisWrapper
{
    public class RedisSetDataException : Exception
    {
        public RedisSetDataException(string message) : base(message)
        {
            
        }
    }
}
