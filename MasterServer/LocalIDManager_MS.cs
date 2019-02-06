using Core.Models.Enums;
using Server.GlobalIDManagers;
using Server.Managers;
using System.Collections.Concurrent;

namespace MasterServer
{
    /// <summary>
    /// Special version of the LocalIDManager for the master server, which directly interacts with the globalIDManager, instead of sending ID requests over a network. Allows for account creation by master server.
    /// </summary>
    internal class LocalIDManager_MS : Server.Managers.ILocalIDManager
    {
        GlobalIDManager _IDSupplier;

        /// <summary>
        /// All objects in the galaxy should pull IDs from this stack
        /// These IDs are received and periodically refreshed from the master server
        /// </summary>
        private ConcurrentStack<int> _freeIDs = new ConcurrentStack<int>();
        private int _minIDCount = 1000;//If minimum number of IDs allowed before a request for more is sent

        public IDTypes IDType { get { return _IDType; } }
        IDTypes _IDType;

        public LocalIDManager_MS(GlobalAccountIDManager globalAccountIDManager, IDTypes IDType)
        {
            
            _IDSupplier = globalAccountIDManager;
            if (_IDSupplier != null)
                _IDSupplier.GetFreeIDs(_minIDCount * 2);
            else
                ConsoleManager.WriteLine("Warning: networkIDSupplier is null in LocalIDManager constructor. This should only be the case when running DBFiller.", ConsoleMessageType.Warning);


            _IDType = IDType;
        }

        /// <summary>
        /// Gives the object a unique free ID, registers the object with various managers and collections
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int PopFreeID()
        {
            int ID;
            if(_minIDCount == 0)
            {
                //Just in case
                _minIDCount = 1000;
            }


            if (_freeIDs.Count < _minIDCount)
            {
                int[] newIDs = _IDSupplier.GetFreeIDs(_minIDCount);
                foreach(int i in newIDs)
                {
                    _freeIDs.Push(i);
                }

            }

            _freeIDs.TryPop(out ID);

            return ID;

        }

        /// <summary>
        /// Use this to return (recycle) an ID
        /// </summary>
        public void PushFreeID(int ID)
        {
            _freeIDs.Push(ID);
        }
        

        /// <summary>
        /// Used to receive IDs from the master server
        /// Separate method to allow reception without thread blocking
        /// </summary>
        /// <param name="newIDs"></param>
        public void ReceiveServerIDs(int[] newIDs)
        {
            foreach (int u in newIDs)
                _freeIDs.Push(u);
        }

    }

   
}
