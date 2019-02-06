using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Utilities
{
    public partial class SerializationUtilities
    {
        static public byte[] SerializeList(List<int> l)
        {
            return l.SelectMany(BitConverter.GetBytes).ToArray();
        }

        static public byte[] SerializeList(HashSet<int> l)
        {
            return l.SelectMany(BitConverter.GetBytes).ToArray();
        }

        static public List<int> DeserializeIntList(byte[] l)
        {
            return Enumerable.Range(0, l.Length / 4)
                             .Select(i => BitConverter.ToInt32(l, i * 4))
                             .ToList();
        }
        
        static public byte[] SerializeList(List<bool> l)
        {
            return l.SelectMany(BitConverter.GetBytes).ToArray();
        }

        static public List<bool> DeserializeBoolList(byte[] l)
        {
            return Enumerable.Range(0, l.Length).Select(i => BitConverter.ToBoolean(l, i)).ToList();

        }
       

    }
}
