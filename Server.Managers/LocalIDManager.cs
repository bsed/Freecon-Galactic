using System.Collections.Concurrent;
using System.Threading;
using Core.Models.Enums;

namespace Server.Managers
{
    /// <summary>
    /// Classes manages ids local to a server instance, but unique to the entire server, automatically requesting new IDs from the network, as necessary.
    /// </summary>
    public class LocalIDManager : ILocalIDManager
    {
        INetworkIDSupplier _networkIDSupplier;

        /// <summary>
        /// All objects in the galaxy should pull IDs from this stack
        /// These IDs are received and periodically refreshed from the master server
        /// </summary>
        private ConcurrentStack<int> _freeIDs = new ConcurrentStack<int>();

        private int _minIDCount = 1000;//If minimum number of IDs allowed before a request for more is sent

        public IDTypes IDType { get { return _IDType; } }
        IDTypes _IDType;

        public int FreeIDCount => _freeIDs.Count;

        public LocalIDManager(INetworkIDSupplier networkIDSupplier, IDTypes IDType)
        {
            _IDType = IDType;

            _networkIDSupplier = networkIDSupplier;
            if (networkIDSupplier != null)
                _networkIDSupplier.RequestFreeIDs(_minIDCount * 2, _IDType);
            else
                ConsoleManager.WriteLine("Warning: networkIDSupplier is null in LocalIDManager constructor. This should only be the case when running DBFiller.", ConsoleMessageType.Warning);


            
        }

        /// <summary>
        /// Gives the object a unique free ID, registers the object with various managers and collections
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int PopFreeID()
        {
            int ID;
            if (!_freeIDs.TryPop(out ID))
            {
                //This should never happen, but just in case
                _networkIDSupplier.RequestFreeIDs(_minIDCount, _IDType);

                ConsoleManager.WriteLine("Warning: GalaxyManager ran out of IDs. This should not be possible, except during initialization", ConsoleMessageType.Warning);
                while (_freeIDs.Count == 0)
                {
                    //This will fail if we move to single threaded, but it isn't supposed to happen anyway.....
                    Thread.Sleep(100);

                }
            }

            
            
            if (_freeIDs.Count < _minIDCount)
                _networkIDSupplier.RequestFreeIDs(_minIDCount, _IDType);
            
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
