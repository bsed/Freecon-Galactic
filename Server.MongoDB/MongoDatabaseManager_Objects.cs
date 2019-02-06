using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Models.Enums;
using Freecon.Models.TypeEnums;
using MongoDB.Bson;
using MongoDB.Driver;
using Server.Database;
using Server.Models;
using Server.Models.Database;
using Server.Models.Interfaces;
using Server.Models.Structures;
using Server.MongoDB;
using Freecon.Server.Core.Interfaces;

namespace Server.Database
{
    public partial class MongoDatabaseManager : IDbIdIoService
    {
        #region Ships

        public async Task<ShipModel> GetShipAsync(int ID)
        {
            var v = (_shipC.Find(s => s.Id == ID).FirstOrDefaultAsync());
            return await v;


            //if (dbs != null)
            //{
            //    switch (dbs.PilotType)
            //    {
            //        case (PilotTypes.Player):
            //            {
            //                PlayerShip p = new PlayerShip(new PlayerShipModel(dbs), ls);
            //                //rm.RegisterObject(p);
            //                return p;
            //            }

            //        default:
            //            throw new NotImplementedException("ShipModel deserialization not implemented for pilot type " + dbs.PilotType);
            //    }

            //}
            //return null;
        }

        public async Task<IEnumerable<ShipModel>> GetShipsAsync(IEnumerable<int> shipIDs)
        {
            var filt = global::MongoDB.Driver.Builders<ShipModel>.Filter.In(s => s.Id, shipIDs);
            var res = await _shipC.FindAsync<ShipModel>(filt);
            return await res.ToListAsync();
        }

        #endregion

        #region Structures

        public async Task<IStructureModel> GetStructureAsync(int ID)
        {
            var filt = Builders<IStructureModel>.Filter.Eq("_id", ID);
            var v = (_structureC.Find(filt).FirstOrDefaultAsync());
            return await v;
        }

        public async Task<IEnumerable<IStructureModel>> GetStructuresAsync(IEnumerable<int> structureIDs)
        {
            var filt = global::MongoDB.Driver.Builders<IStructureModel>.Filter.In("_id", structureIDs);
            var res = await _structureC.FindAsync<IStructureModel>(filt);
            return await res.ToListAsync();
        }

        #endregion

        #region Players

        /// <summary>
        /// Gets the raw player model
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public virtual async Task<PlayerModel> GetPlayerAsync(int ID)
        {
            var dbp = (_playerC.Find(s => s.Id == ID).FirstOrDefaultAsync());
            return await dbp;
        }

        public virtual async Task<PlayerModel> GetPlayerAsync(string username)
        {
            var dbp = (_playerC.Find(s => s.Username == username).FirstOrDefaultAsync());
            return await dbp;
        }

        public virtual async Task<IEnumerable<PlayerModel>> GetPlayersAsync(IEnumerable<int> ids)
        {
            var filt = global::MongoDB.Driver.Builders<PlayerModel>.Filter.In(e => e.Id, ids);
            var res = await _playerC.FindAsync<PlayerModel>(filt);
            return await res.ToListAsync();
        }

        #endregion

        #region Areas

        /// <summary>
        /// Reads an area from the DB
        /// If the areatype is  PSystem, loads all associated nested objects with each system (e.g. planets, ships, players...). Probably subject to change.
        /// </summary>
        /// <returns></returns>
        public async Task<AreaModel> GetAreaAsync(int ID, AreaTypes expectedType = AreaTypes.Any)
        {
            var a = await (_areaC.Find(s => s.Id == ID).FirstOrDefaultAsync());
            //Unknown descriminator exception? Register the class in _initData()

            if (expectedType != AreaTypes.Any && expectedType != a.AreaType)
                throw new WrongAreaTypeException(
                    "Error: AreaModel loaded from database does not match requested AreaModel.Type. Expected type: " +
                    expectedType + "; loaded type: " + a.AreaType);

            return a;
        }

        public async Task<IEnumerable<AreaModel>> GetAreasAsync(IEnumerable<int> IDs)
        {
            var areaFilt = global::MongoDB.Driver.Builders<AreaModel>.Filter.In(l => (l.Id), IDs);

            var res = await _areaC.FindAsync(areaFilt);
            return await res.ToListAsync();
        }

        public async Task<IEnumerable<PlanetLayout>> GetLayoutsAsync(IEnumerable<int> IDs)
        {
            var areaFilt = global::MongoDB.Driver.Builders<PlanetLayout>.Filter.In(l => (l.Id), IDs);

            var res = await _layoutC.FindAsync(areaFilt);
            return await res.ToListAsync();
        }

        /// <summary>
        /// Loads all associated nested objects with each system (e.g. planets, ships, players...)
        /// </summary>
        /// <param name="pl"></param>
        /// <param name="al"></param>
        /// <param name="sl"></param>
        /// <param name="mm"></param>
        /// <returns></returns>
        public async Task<IEnumerable<PSystemModel>> GetAllSystemsAsync()
        {


            //foreach (var a in _areaC.FindAsync<PSystemModel>(Builders<AreaModel>.Filter.Eq(e => e.Type, AreaTypes.System)).Result.ToListAsync().Result)
            //{
            //    retSys.Add(Deserializer.DeserializePSystem(a, ls, rm, this));
            //}

            var res =
                await _areaC.FindAsync<PSystemModel>(global::MongoDB.Driver.Builders<AreaModel>.Filter.Eq(e => e.AreaType, AreaTypes.System));

            return await res.ToListAsync();



        }

        public async Task<IEnumerable<AreaModel>> GetAllAreas()
        {
            var res = await _areaC.FindAsync<AreaModel>(global::MongoDB.Driver.Builders<AreaModel>.Filter.Exists(p => p.AreaType));

            return await res.ToListAsync();
        }

        public async Task<IEnumerable<AreaModel>> GetAllAreas(AreaTypes areaTypes)
        {
            var res = await _areaC.FindAsync<AreaModel>(global::MongoDB.Driver.Builders<AreaModel>.Filter.Eq(e => e.AreaType, areaTypes));

            return await res.ToListAsync();
        }

        public async Task<IEnumerable<PlanetModel>> GetPlanetsAsync(IEnumerable<int> planetIDs)
        {
            //List<DBPlanet> result = new List<DBPlanet>(planetIDs.Count());

            var filt = global::MongoDB.Driver.Builders<AreaModel>.Filter.In(a => (a.Id), planetIDs);

            var res = await _areaC.FindAsync<PlanetModel>(filt);

            return await res.ToListAsync(); //Not sure if I need both awaits

        }

        #endregion

        #region Accounts

        public async Task<AccountModel> GetAccountAsync(int ID)
        {
            return await (_accountC.Find(ss => ss.Id == ID).FirstOrDefaultAsync());
        }

        public async Task<AccountModel> GetAccountAsync(string username)
        {
            return await (_accountC.Find(ss => ss.Username == username).FirstOrDefaultAsync());
        }

        public async Task<IEnumerable<AccountModel>> GetAccountsAsync(IEnumerable<int> ids)
        {
            var filt = global::MongoDB.Driver.Builders<AccountModel>.Filter.In(e => e.Id, ids);
            var res = await _accountC.FindAsync<AccountModel>(filt);
            return await res.ToListAsync();

        }

        public async Task<IEnumerable<AccountModel>> GetAllAccountsAsync()
        {
            var res = await _accountC.FindAsync<AccountModel>(new BsonDocument());
            return await res.ToListAsync();
        }

        #endregion

        #region Teams

        public async Task<TeamModel> GetTeamAsync(int id)
        {
            return await _teamC.Find(t => t.Id == id).FirstAsync();
        }

        public async Task<DeleteResult> DeleteTeam(int id)
        {
            return await _teamC.DeleteOneAsync(t => t.Id == id);
        }

        #endregion

        #region TradeRecordModels

        public async Task<TradeRecordModel> GetTradeRecordModelAsync(int ID)
        {
            var v = (_tradeRecordModelC.Find(s => s.Id == ID).FirstOrDefaultAsync());
            return await v;


            //if (dbs != null)
            //{
            //    switch (dbs.PilotType)
            //    {
            //        case (PilotTypes.Player):
            //            {
            //                PlayerShip p = new PlayerShip(new PlayerShipModel(dbs), ls);
            //                //rm.RegisterObject(p);
            //                return p;
            //            }

            //        default:
            //            throw new NotImplementedException("ShipModel deserialization not implemented for pilot type " + dbs.PilotType);
            //    }

            //}
            //return null;
        }

        public async Task<IEnumerable<TradeRecordModel>> GetTradeRecordModelsAsync(IEnumerable<int> Ids)
        {
            var filt = global::MongoDB.Driver.Builders<TradeRecordModel>.Filter.In(s => s.Id, Ids);
            var res = await _tradeRecordModelC.FindAsync<TradeRecordModel>(filt);
            return await res.ToListAsync();
        }


        #endregion

        #region IdData

        public async Task<IDbIdData> GetIdDataAsync(IDTypes idType)
        {
            return await (await _idDataC.FindAsync(Builders<IDbIdData>.Filter.Eq(c => c.IdType, idType))).FirstOrDefaultAsync();
        }

        public IDbIdData GetIdData(IDTypes idType)
        {
            return _idDataC.FindSync(Builders<IDbIdData>.Filter.Eq("_id", idType)).FirstOrDefault();
        }

        public async Task SaveIdDataAsync(IDbIdData idData)
        {
            await _idDataC.ReplaceOneAsync(Builders<IDbIdData>.Filter.Eq("_id", idData.IdType), idData, new UpdateOptions {IsUpsert = true});
        }

        #endregion
    }
}
