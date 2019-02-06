//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Server.Interfaces;
//using Server.Models;
//using SRServer.Services;

//namespace Freecon.Server.Core.Objects
//{
//    //Might finish this later, suddenly not sure if it's worth the effort.
//    //Envisioned as a generic cache class, to replace _playerCache, _shipCache, etc which prevent needless calls to the db and are currently implemented


//    /// <summary>
//    /// A helper class which acts as a chache to prevent loading of models unless explicitly required.
//    /// //TODO: Make generic locator class to allow for truely generic cache
//    /// </summary>
//    public abstract class ModelCache<TModel>
//        where TModel : ISerializable
//    {
//        protected Dictionary<int, TModel> _cache;
//        public bool IsCacheReady { get; protected set; }
        
//        public abstract TModel GetObject(int id);

//        public abstract void RefreshAll();

//        public void AddObject(TModel obj)
//        {
//            if (!_cache.ContainsKey(obj.Id))
//            {
//                _cache.Add(obj.Id, obj);
//            }
//        }

//        public abstract void Clear();

//        protected ModelCache()
//        {
//            _cache = new Dictionary<int, TModel>();
//        }

//        /// <summary>
//        /// Returns all the objects in the cache. It !IsCacheReady, loads all objects through locator
//        /// </summary>
//        /// <param name="objectIDs"></param>
//        /// <returns></returns>
//        public abstract Dictionary<int, TModel> GetAll(List<int> objectIDs);
//    }


//    public class PlayerCache : ModelCache<Player>
//    {
//        IPlayerLocator _playerLocator;

//        /// <summary>
//        /// Returns all the objects in the cache. It !IsCacheReady, loads all objects through locator
//        /// </summary>
//        /// <param name="objectIDs"></param>
//        /// <returns></returns>
//        public override Dictionary<int, Player> GetAll(List<int> objectIDs)
//        {
//            Dictionary<int, Player> retdic;

//            if (IsCacheReady)
//                retdic = new Dictionary<int, Player>(_cache);
//            else
//            {
//                _cache.Clear();
//                retdic = new Dictionary<int, Player>(objectIDs.Count);
//                var sl = _playerLocator.GetPlayersAsync(objectIDs).Result;

//                foreach (var s in sl)
//                {
//                    retdic.Add(s.Id, s);
//                    _cache.Add(s.Id, s);
//                }
//                IsCacheReady = true;
//            }

//            return retdic;

//        }
//    }

//    public class ShipCache : ModelCache<IShip>
//    {
//        IShipLocator _shipLocator;


//        /// <summary>
//        /// Returns all the objects in the cache. It !IsCacheReady, loads all objects through locator
//        /// </summary>
//        /// <param name="objectIDs"></param>
//        /// <returns></returns>
//        public override Dictionary<int, IShip> GetAll(List<int> objectIDs)
//        {
//            Dictionary<int, IShip> retdic;

//             if (IsCacheReady)
//                retdic = new Dictionary<int, IShip>(_cache);
//            else
//            {
//                _cache.Clear();
//                retdic = new Dictionary<int, IShip>(objectIDs.Count);
//                var sl = _shipLocator.GetShipsAsync(objectIDs).Result;
                
//                foreach (var s in sl)
//                {
//                    retdic.Add(s.Id, s);
//                    _cache.Add(s.Id, s);
//                }
//                IsCacheReady = true;
//            }

//            return retdic;

//        }
//    }

//}
