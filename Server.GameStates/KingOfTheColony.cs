//using Freecon.Models.TypeEnums;
//using Server.Managers;
//using Server.Managers.Factories;
//using Server.Models;
//using SRServer.Services;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Freecon.Server.GameStates
//{
//    public class KingOfTheColony:ServerGameState<KingOfTheColonyModel>
//    {
//        public int NumTeams { get; set; }

//        ShipFactory _shipFactory;
//        KillManager _killManager;
//        IAreaLocator _areaLocator;
//        IPlayerLocator _playerLocator;

//        public KingOfTheColony(KingOfTheColonyStats stats, int spawnSystemId, int captureSystemId, ShipFactory shipFactory, KillManager killManager, IAreaLocator areaLocator, IPlayerLocator playerLocator)
//        {
//            _model.SystemIDs.Add(spawnSystemId);
//            _model.SystemIDs.Add(captureSystemId);
//            _shipFactory = shipFactory;
//            _killManager = killManager;
//            _areaLocator = areaLocator;
//            _playerLocator = playerLocator;
//        }

//        public Task Reset()
//        {
//            Task t = new Task(() =>
//            {

//                //List<Player> players = _playerLocator.GetPlayersAsync()

//                //Create ships in spawn area
//                foreach (var id in _model.PlayerIds)
//                {
//                //    _shipFactory.DeleteShip(
//                }

//            });

//            t.Start();
//            return t;
//        }

//        public override bool AddPlayer(int playerId)
//        {           
//            var p = _playerLocator.GetPlayerAsync(playerId).Result;
//            if (p == null)//Player is non-local
//            {
             
//            }

//            if (base.AddPlayer(playerId))
//            {
                
//            }
//            else
//                return false;
//        }

//        protected override ShipCreationProperties GetDefaultShipProperties(float posX, float posY, int areaId)
//        {
//            List<WeaponTypes> weapons = new List<WeaponTypes> { WeaponTypes.Laser };
//            var props = new ShipCreationProperties(posX, posY, areaId, PilotTypes.Player, ShipTypes.Penguin, weapons);

//            return props;
//        }

//    }
//}
