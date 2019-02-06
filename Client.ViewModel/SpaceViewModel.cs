using Freecon.Client.Core.Objects;
using Freecon.Models.TypeEnums;
using Core.Interfaces;
using Freecon.Client.Managers;
using Freecon.Client.Managers.Networking;
using Freecon.Client.Objects.Structures;
using System.Collections.Generic;

namespace Freecon.Client.ViewModel
{
    public class SpaceViewModel : PlayableViewModel
    {
        protected double _lastTimeStamp;

        protected ClientManager _clientManager;
        protected ProjectileManager _projectileManager;
        protected ClientShipManager _clientShipManager;
        protected WarpHoleManager _warpHoleManager;
        protected MessageService_ToServer _messageManager;

        public bool StructurePlacementModeEnabled { get; set; }

        // Debugging, to give ships motion, although these could be used in the game in interesting ways
        public List<GravityObject> GravityObjects { get; set; }

        private IList<Turret> _turrets;
        public IList<Turret> Turrets
        {
            get { return _turrets; }
            private set { _turrets = value; }
        }

        public SpaceViewModel(
            ClientShipManager clientShipManager
            )
        {
            _clientShipManager = clientShipManager;

            GravityObjects = new List<GravityObject>();
            Turrets = new List<Turret>();

        }


        public override void Clear()
        {
            base.Clear();
            _turrets.Clear();
        }

        public void RemoveTurret(Turret t)
        {
            Turrets.Remove(t);
        }

        public void ToggleStructurePlacementMode(string ignored)
        {
            StructurePlacementModeEnabled = !StructurePlacementModeEnabled;
        }

        public override void Update(IGameTimeService gameTime)
        {
#if ADMIN
            foreach (GravityObject g in _gravityObjects) // Testing stuff
            {
                g.Gravitate(_clientShipManager._shipList.Values, _clientShipManager.PlayerShip);
            }
#endif



            foreach (var s in Structures)
            {
                switch (s.StructureType)
                {
                    case StructureTypes.LaserTurret:
                        ((Turret)s).Update(gameTime);
                        break;

                    default:
                        s.Update(gameTime);
                        break;
                }
            }

            //switch (State)
            //{
            //    case GameStates.updating:
            //        break;
            //    case GameStates.transitional:
            //        _bus.Publish(new MChangeStateMessage(GameStates.Port));
            //        break;
            //}


            _lastTimeStamp = gameTime.TotalMilliseconds;
        }
    }
}
