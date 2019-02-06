using System;
using System.Collections.Generic;

namespace Core.Networking
{
    /// <summary>
    /// Constructor and destructor are thread safe.
    /// </summary>
    public class MessageHandlerID
    {
        public int Value { get { return _value; } }
        int _value;

        static int _nextFreeID = 0;
        static HashSet<int> _usedIDs = new HashSet<int>();

        object ADDLOCK = new object();
        object REMOVELOCK = new object();

        /// <summary>
        /// Automatically generates a unique ID via a static member. Thead safe.
        /// </summary>
        public MessageHandlerID()
        {
            lock (ADDLOCK)
            {
                while (_usedIDs.Contains(_nextFreeID))
                {
                    _nextFreeID++;
                }
                _value = _nextFreeID;
                _usedIDs.Add(_value);
                _nextFreeID++;
            }
        }

        /// <summary>
        /// Instantiates a MessageHandlerID if the given ID is not in use. Throws an exception if the ID is in use. Thread safe.
        /// </summary>
        /// <param name="ID"></param>
        public MessageHandlerID(int ID)
        {
            lock (ADDLOCK)
            {
                if (_usedIDs.Contains(ID))
                {
                    throw new InvalidOperationException("ID is already in use.");
                }
                else
                {
                    _usedIDs.Add(ID);
                    _value = ID;
                }
            }
        }

        public static implicit operator int(MessageHandlerID m)
        {
            return m.Value;
        }

        ~MessageHandlerID()
        {
            lock (REMOVELOCK)
            {
                _usedIDs.Remove(_value);
            }
        }


    }
}
