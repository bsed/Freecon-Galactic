using Freecon.Client.Interfaces;
using Core.Interfaces;
using Freecon.Client.Objects;
using Freecon.Client.Objects.Pilots;
using Freecon.Client.Managers.Networking;
using Freecon.Models.TypeEnums;
using Freecon.Core.Utils;
using Freecon.Client.Mathematics;

namespace Freecon.Client.Managers
{
    public class PlayerShipManager : ISynchronousManager
    {
        MessageService_ToServer _messageService;

        float _lastRequestTime;
        float _minimumRequestPeriod = 50;//Prevent spam

        private Pilot _playerPilot;
        public Pilot PlayerPilot
        {
            get { return _playerPilot; }
            set { _playerPilot = value; }
        }

        private Ship _playerShip;
        public Ship PlayerShip
        {
            get { return _playerShip; }
            set { _playerShip = value; }
        }
               

        public PlayerShipManager(MessageService_ToServer messageService)
        {
            _messageService = messageService;
        }

        public void Update(IGameTimeService gameTime)
        {
            if(KeyboardManager.PlaceTurret.IsBindTapped())
            {
                if(PlayerShip != null && PlayerShip.Cargo.IsCargoInHolds(StatefulCargoTypes.LaserTurret, 1) && TimeKeeper.MsSinceInitialization - _lastRequestTime > _minimumRequestPeriod)
                {
                    PlayerShip.EnterMode = false;

                    _messageService.SendStructurePlacementRequest(StructureTypes.LaserTurret, PlayerShip.Position,PlayerShip.Id);

                    _lastRequestTime = TimeKeeper.MsSinceInitialization;
                }

            }

            if(KeyboardManager.PlaceMine.IsBindTapped())
            {
                if (PlayerShip != null && PlayerShip.Cargo.IsCargoInHolds(StatefulCargoTypes.DefensiveMine, 1) && TimeKeeper.MsSinceInitialization - _lastRequestTime > _minimumRequestPeriod)
                {
                    PlayerShip.EnterMode = false;
                 
                    _messageService.SendStructurePlacementRequest(StructureTypes.DefensiveMine, PlayerShip.Position, PlayerShip.Id);

                    _lastRequestTime = TimeKeeper.MsSinceInitialization;
                }
            }
            

        }

        public void Draw(Camera2D camera)
        {

        }

       
       
            
    }
}
