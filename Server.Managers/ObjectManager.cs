using Server.Database;
using SRServer.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Freecon.Core.Interfaces;

namespace Server.Managers
{
    /// <summary>
    /// Generic ObjectManager base class which performs basic operations common to most object-storing managers (e.g. PlayerManager)
    /// </summary>
    /// <typeparam name="ObjectType"></typeparam>
    /// <typeparam name="ObjectModelType"></typeparam>
    public abstract class ObjectManager<ObjectType, ObjectModelType> : IObjectLocator<ObjectType>
        where ObjectType : ISerializable
        where ObjectModelType : IDBObject
    { 
        protected ConcurrentDictionary<int, ObjectType> _objects; //All of the Objects in the galaxy. Convenient for ObjectType lookups.

        protected IDatabaseManager _databaseManager;

        protected float _lastUpdateTimeStamp { get; set; }

        public ObjectManager(IDatabaseManager dbm)
        {
            _databaseManager = dbm;

            _objects = new ConcurrentDictionary<int, ObjectType>();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentTime">Elapsed ms since server start</param>
        public virtual async Task Update(float currentTime)
        {
            _lastUpdateTimeStamp = currentTime;
        }

        public virtual void RegisterObject(ObjectType s)
        {

            _objects.AddOrUpdate(s.Id, s, (k, v) => s);

        }

        public virtual void DeregisterObject(ObjectType Object)
        {
            if (Object == null)
                return;

            ObjectType tempObject;

          
            _objects.TryRemove(Object.Id, out tempObject);
            
        }


        public virtual void DeregisterObject(int ID)
        {
            ObjectType s;
            _objects.TryRemove(ID, out s);

        }
        /// <summary>
        /// This will probably be removed, just for testing db 
        /// </summary>
        /// <returns></returns>
        public virtual List<ObjectType> GetAllObjects()
        {
            return _objects.Values.ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="fetchFromDB">If true, ls cannot be null</param>
        /// <param name="ls">Cannot be null if fetchFromDB is true</param>
        /// <param name="persistFetch">If true, fetched instance is stored/updated in the manager's list of Objects</param>
        /// <returns></returns>
        public virtual async Task<ObjectType> GetObjectAsync(int? ID, bool fetchFromDB = false, LocatorService ls = null, bool persistFetch = true)
        {
            if (ID == null)
                return default(ObjectType);

            ObjectType s = default(ObjectType);//Default value if s isn't found

            if (fetchFromDB)
            {
                if (ls == null)
                {
                    throw new Exception("Cannot fetch a ObjectType from the database without a reference to a LocatorService object.");
                }

                var sm = await _databaseManager.GetObjectAsync((int)ID, typeof(ObjectModelType));
                s = _instantiateObject(sm, ls);


                if (persistFetch && s != null)
                {
                    RegisterObject(s);
                }
            }
            else
            {
                if (_objects.ContainsKey((int)ID))
                    s = _objects[(int)ID];
            }

            return s;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="IDs"></param>
        /// <param name="fetchFromDB">If true, ls cannot be null</param>
        /// <param name="ls">Cannot be null if fetchFromDB is true</param>
        /// <param name="persistFetch">If true, fetched instance is stored/updated in the manager's list of Objects</param>
        /// <returns></returns>
        public virtual async Task<ICollection<ObjectType>> GetObjectsAsync(List<int> IDs, bool fetchFromDB = false, LocatorService ls = null, bool persistFetch = true)
        {
            ICollection<ObjectType> retObjects = new List<ObjectType>();

            if (fetchFromDB)
            {
                if (ls == null)
                {
                    throw new Exception("Cannot fetch a ObjectType from the database without a reference to a LocatorService object.");
                }

                var ObjectModels = await _databaseManager.GetObjectsAsync(IDs, typeof(ObjectType));

                foreach (var s in ObjectModels)
                {
                    var newObject = _instantiateObject(s, ls);
                    retObjects.Add(newObject);
                    if (persistFetch)
                    {
                        RegisterObject(newObject);
                    }

                }
            }
            else
            {
                foreach (var id in IDs)
                {
                    if (_objects.ContainsKey(id))
                    {
                        retObjects.Add(_objects[id]);
                    }
                }
            }

            return retObjects;
        }

        /// <summary>
        /// Instantiates and returns an object of type ObjectType from the IDBObject, which must be of type ObjectModelType 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="ls"></param>
        /// <returns></returns>
        protected abstract ObjectType _instantiateObject(IDBObject s, LocatorService ls);
        

        




    }
}
