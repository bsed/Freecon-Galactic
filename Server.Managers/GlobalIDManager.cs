using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using Freecon.Server.Configs;
using Core.Models.Enums;
using Freecon.Server.Core.Interfaces;
using Server.Managers;
using Server.MongoDB;

namespace Server.GlobalIDManagers
{
    /// <summary>
    /// Class manages all types of unique IDs (galaxyIDs, teamIDs, etc), ensures they are synchronized among slave servers.
    /// There should only be one for each IDTypes per universe.
    /// Currently designed to be used as a single instance (per IdType), consumed only by the MasterServer constructor
    /// </summary>
    public abstract class GlobalIDManager
    {
        protected object IDLOCK = new object();

        protected ConcurrentStack<int> _freeIDs = new ConcurrentStack<int>();
        protected int _minIDCount = 1000;//Minimum number of IDs allowed before more are generated

        //IDs which have been sent to a slave. They will not be used immediately. We may lose track of certain ids if servers don't return unused ids before shutdown/crash
        protected ConcurrentStack<int> _generatedIDs = new ConcurrentStack<int>();

        protected HashSet<int> _reservedIDs = new HashSet<int>();

        protected int _lastIdGenerated;//Keeps track of the highest ID either in use or in the FreeIDs stack above, to generate unique IDs when stack is empty

        public IDTypes IDType { get; protected set; }

        protected IDbIdIoService _dbIdIoService;
       
        public GlobalIDManager(IDbIdIoService dbIdIoService, IDTypes idType)
        {
            _dbIdIoService = dbIdIoService;
            IDType = idType;

            var idData = _dbIdIoService.GetIdData(idType);

            if (idData == null)
            {
                throw new Exception("ID data for IdType " + idType + " not initialized in database.");
            }
            
            _lastIdGenerated = idData.LastIdGenerated;

            _reservedIDs = idData.ReservedIds;

            _freeIDs = new ConcurrentStack<int>(idData.FreeIDs);
            _generatedIDs = new ConcurrentStack<int>(idData.UsedIDs);


        }

        public int[] GetFreeIDs(int numIDs)
        {
            lock (IDLOCK)
            {
                if (_freeIDs.Count < numIDs + _minIDCount)
                    _generateNewIDs(numIDs + _minIDCount);


                int[] outArr = new int[numIDs];
                _freeIDs.TryPopRange(outArr);

                return outArr;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ID"></param>
        /// <returns>Returns true if the ID was already reserved, false otherwise</returns>
        public bool ReserveID(int ID)
        {
            lock (IDLOCK)
            {
                bool retVal = _reservedIDs.Contains(ID);
                _reservedIDs.Add(ID);

                return retVal;
            }
        }

        public HashSet<int> GetReservedIDs()
        {
            return new HashSet<int>(_reservedIDs);
        }


        void _generateNewIDs(int numIDs)
        {
            lock (IDLOCK)
            {
                try
                {
                    ConsoleManager.WriteLine("Generating new ids of type " + IDType, ConsoleMessageType.Notification);

                    int n = checked(numIDs + _minIDCount);
                    //If this statement throws an exception, congratulations! Freecon has grown
                    //large enough to run out of 32bit galaxy IDs. Now you as the developer
                    //are in line for the pleasure of implementing 64bit galaxy IDs! Have fun,
                    //and try not to kill yourself!

                    for (int i = _lastIdGenerated + 1; i < numIDs + _lastIdGenerated + 1; i++)
                    {
                        if (_reservedIDs.Contains(i))
                        {
                            n = checked(numIDs + _minIDCount); //Throws exception if numIDs + _minIDCount > int.MAX
                            numIDs++;
                            continue;
                        }

                        _freeIDs.Push(i);


                    }
                    _lastIdGenerated = numIDs + _lastIdGenerated;

                    var idData = new IdData(IDType, _lastIdGenerated, new HashSet<int>(_freeIDs), new HashSet<int>(_generatedIDs), new HashSet<int>(_reservedIDs));
                    _dbIdIoService.SaveIdDataAsync(idData);
                    
                }
                catch
                {
                    throw new Exception("Ran out of 32 bit ints for IDs. Time for a 64 bit server!");
                }
            }
        }


    }

    public class GlobalGalaxyIDManager : GlobalIDManager
    {
        /// <summary>
        /// Remember to reserve, at a minimum, the solID in reservedIDs. c.ReservedIDs takes care of this as of this writing.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="reservedIDs"></param>
        public GlobalGalaxyIDManager(IDbIdIoService dbIdIoService, GalacticProperties c)
            : base(dbIdIoService, IDTypes.GalaxyID)
        { }
    }


    public class GlobalTeamIDManager : GlobalIDManager
    {
        public GlobalTeamIDManager(IDbIdIoService dbIdIoService, GalacticProperties c)
            : base(dbIdIoService, IDTypes.TeamID)
        { }
    }

    public class GlobalAccountIDManager : GlobalIDManager
    {
        public GlobalAccountIDManager(IDbIdIoService dbIdIoService, GalacticProperties c)
            : base(dbIdIoService, IDTypes.AccountID)
        { }
    }

    public class GlobalTransactionIDManager : GlobalIDManager
    {
        public GlobalTransactionIDManager(IDbIdIoService dbIdIoService, GalacticProperties c)
            : base(dbIdIoService, IDTypes.AccountID)
        {
        }
    }

}
