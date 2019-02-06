using Freecon.Models.TypeEnums;
using Core.Interfaces;
using Freecon.Client.Managers;
using Freecon.Client.Managers.Networking;
using Freecon.Client.Objects;
using Freecon.Client.Objects.Structures;
using System.Collections.Generic;

namespace Freecon.Client.ViewModel
{
    public class PlanetViewModel : PlayableViewModel
    {
        // This is temporary
        public bool ColonizeMode { get; set; }

        // Maybe not temporary after the new IoC and bus framework
        public bool StructurePlacementMode { get; set; }

        protected double _lastTimeStamp;
        protected ClientManager _clientManager;
        protected ProjectileManager _projectileManager;
        protected ClientShipManager _clientShipManager;
        protected WarpHoleManager _warpHoleManager;
        protected MessageService_ToServer _messageManager;

        public HashSet<int> ColonyTeamIDs { get; set; }

        public PlanetLevel Level { get; set; }

        public IList<Turret> Turrets { get; protected set; }

        public PlanetViewModel(ClientShipManager clientShipManager)
        {
            _clientShipManager = clientShipManager;

            ColonyTeamIDs = new HashSet<int>();
            Turrets = new List<Turret>();

        }

        public void Clear()
        {
            ColonizeMode = false;
            StructurePlacementMode = false;
            _structures.Clear();
            ColonyTeamIDs = new HashSet<int>();
        }

        public void RemoveTurret(Turret t)
        {
            Turrets.Remove(t);
        }

        /// <summary>
        /// Toggles colonize mode, ensures that no other modes are active
        /// This will need to be changed to allow for placement of other structures
        /// </summary>
        public void ToggleColonizeMode()
        {
            ColonizeMode = !ColonizeMode;

            if (ColonizeMode)
                StructurePlacementMode = false;
        }

        /// <summary>
        /// Toggles colonize mode, ensures that no other modes are active
        /// This will need to be changed to allow for placement of other structures
        /// </summary>
        public void ToggleStructurePlacementMode()
        {
            StructurePlacementMode = !StructurePlacementMode;

            if (StructurePlacementMode)
                ColonizeMode = false;
        }

        public override void Update(IGameTimeService gameTime)
        {
#if ADMIN
            foreach (GravityObject g in _gravityObjects) // Testing stuff
            {
                g.Gravitate(_clientShipManager._shipList.Values, _clientShipManager.PlayerShip);
            }
#endif

            if (KeyboardManager.ColonizeMode.IsBindTapped() && !ColonizeMode)
            {
                ColonizeMode = true;
            }
            else if (KeyboardManager.ColonizeMode.IsBindTapped() && ColonizeMode)
            {
                ColonizeMode = false;
            }

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


            _lastTimeStamp = gameTime.TotalMilliseconds;
        }
    }
}
