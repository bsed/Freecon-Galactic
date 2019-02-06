using Core.Models;
using Freecon.Models.TypeEnums;
using Server.Models;
using Server.Models.Database;
using Server.Models.Interfaces;
using Server.Models.Structures;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Freecon.Core.Interfaces;
using Freecon.Server.Core.Interfaces;
using MongoDB.Driver;

namespace Server.Database
{
    public interface IDatabaseManager : IDBWriter, IDbIdIoService
    {
        /// <summary>
        /// Searches for the given id in the appropriate database/collection/table, determined by objectType
        /// </summary>
        /// <param name="id"></param>
        /// <param name="objectType"></param>
        /// <returns></returns>
        Task<IDBObject> GetObjectAsync(int id, Type objectModelType);

        Task<IEnumerable<IDBObject>> GetObjectsAsync(IEnumerable<int> ids, Type objectModelType);


        Task<IStructureModel> GetStructureAsync(int ID);
        
        Task<IEnumerable<IStructureModel>> GetStructuresAsync(IEnumerable<int> structureIDs);

        /// <summary>
        /// Returns null if ship is not found
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<ShipModel> GetShipAsync(int id);

        Task<IEnumerable<ShipModel>> GetShipsAsync(IEnumerable<int> ids);


        /// <summary>
        /// Reads the AreaModel with the corresponding ID from the database
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="expectedType">If Specified, throws an exception if the loaded area does not match the expected type</param>
        /// <returns></returns>
        Task<AreaModel> GetAreaAsync(int ID, AreaTypes expectedType = AreaTypes.Any);

        Task<IEnumerable<AreaModel>> GetAreasAsync(IEnumerable<int> ids);

        /// <summary>
        /// Does not load or register nested objects within systems (e.g. planets)
        /// </summary>
        /// <param name="pl"></param>
        /// <param name="al"></param>
        /// <param name="sl"></param>
        /// <param name="mm"></param>
        /// <returns></returns>
        Task<IEnumerable<PSystemModel>> GetAllSystemsAsync();

        Task<IEnumerable<AreaModel>> GetAllAreas();

        Task<IEnumerable<AreaModel>> GetAllAreas(AreaTypes areaTypes);

        /// <summary>
        /// Returns null if player not found
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<PlayerModel> GetPlayerAsync(int id);

        Task<IEnumerable<PlayerModel>> GetPlayersAsync(IEnumerable<int> ids);

        Task<PlayerModel> GetPlayerAsync(string username);

        /// <summary>
        /// Returns null if account not found
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<AccountModel> GetAccountAsync(int id);

        /// <summary>
        /// Returns null if account not found
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<AccountModel> GetAccountAsync(string username);

        Task<IEnumerable<AccountModel>> GetAccountsAsync(IEnumerable<int> ids);

        Task<IEnumerable<AccountModel>> GetAllAccountsAsync();

        Task<TeamModel> GetTeamAsync(int id);

        Task<IEnumerable<IDBObject>> GetHandoffModelsAsync(ModelTypes modelType);
        
        Task<IEnumerable<ShipStats>> GetStatsFromDBAsync();

        Task<IEnumerable<PlanetModel>> GetPlanetsAsync(IEnumerable<int> planetIDs);
        
        Task<IEnumerable<PlanetLayout>> GetAllLayoutsAsync();

        Task<IEnumerable<PlanetLayout>> GetLayoutsAsync(IEnumerable<int> IDs);


    }
}
