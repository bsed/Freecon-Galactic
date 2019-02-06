using Freecon.Client.Core.Extensions;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Freecon.Models.TypeEnums;
using Freecon.Client.GameStates;
using Freecon.Client.ViewModel;
using Freecon.Client.Core.Factories;
using Freecon.Client.Core.States;
using Freecon.Client.Managers.Space;
using Freecon.Client.Core.States.Components;
using Freecon.Core.Networking.Models;
using Freecon.Client.Managers.Networking;
using Freecon.Client.Core.Managers;
using Freecon.Core.Networking.Models.Objects;
using Core.Networking;
using Freecon.Client.View.CefSharp;
using Freecon.Core.Networking.Interfaces;

namespace Freecon.Client.Managers.States
{
    public class SpaceStateManager : PlayableGameStateClient<SpaceViewModel, SpaceWebView>
    {
        public List<string> ConnectionInfo;

        protected readonly SpaceManager _spaceManager;


        public string SystemName { get; protected set; }

        public SpaceStateManager(
            MessageHandlerID messageHandlerId,
            IClientPlayerInfoManager clientPlayerInfoManager,
            IGlobalGameUISingleton globalGameUiSingleton,
            CollisionManager collisionManager,
            INetworkingService networkingService,
            PhysicsManager physicsManager,
            PlayerShipManager playerShipManager,
            ProjectileManager projectileManager,
            SelectionManager selectionManager,
            SimulationManager simulationManager,
            ClientShipManager clientShipManager,
            SpaceManager spaceManager,
            StructureFactoryManager structureFactoryManager,
            TargetingService targetingService,
            TeamManager teamManager,
            WarpHoleManager warpHoleManager,
            MessageService_ToServer messageService,
            FloatyAreaObjectManager floatyAreaObjectManager)
            : base(messageHandlerId, clientPlayerInfoManager, collisionManager, globalGameUiSingleton, networkingService, physicsManager, playerShipManager,
                projectileManager, selectionManager, clientShipManager, simulationManager, structureFactoryManager,
                warpHoleManager, teamManager, targetingService, floatyAreaObjectManager, messageService, GameStateType.Space, new SpaceViewModel(clientShipManager)
            )
        {
            _spaceManager = spaceManager;
            _synchronousUpdateList.Add(_playerShipManager);

        }


        protected override void _playableStateManager_MessageReceived(object sender, NetworkMessageContainer e)
        {
            base._playableStateManager_MessageReceived(sender, e);

            switch (e.MessageType)
            {
                case MessageTypes.StarSystemData:
                {
                    Clear();
                    ReadIncomingSystem(e.MessageData as SystemEntryData);
                    break;
                }
                case MessageTypes.WarpApproval:
                {
                    Clear();
                    ReadIncomingSystem(e.MessageData as SystemEntryData);
                    _clientShipManager.PlayerShip.LinearVelocity = new Vector2(0, 0);//Should probably move this
                    break;
                }

            }
        }
        
        public override void Activate(IGameState previous)
        {
            
            if (!(_areaDataReceived && _clientShipManager.PlayerShip != null))
            {
                //Wait for system to load
                Status = GameStateStatus.Activating;
            }
            else
            {
                base.Activate(previous);              
                                            
            }
            
        }

        public override void Clear()
        {
            base.Clear();
            _spaceManager.Reset();
            _targetingService.Clear(_clientShipManager.PlayerShip);
            _teamManager.Clear();

        }

       

        public void ToggleStructurePlacementMode()
        {
            //ViewModel.ToggleStructurePlacementMode();
        }

        /// <summary>
        /// Reads the incoming system from message.
        /// Todo: Kill me please.
        /// </summary>
        /// <param name="msg">The lidgren networking message.</param>
        public SystemEntryData ReadIncomingSystem(SystemEntryData data)
        {
            if (data == null)
            {
                ClientLogger.LogError("Trying to read incoming system but received null data.");
                return null;
            }

            SystemName = data.AreaName;
            _clientShipManager.CurrentAreaId = data.Id;
            _spaceManager.InitializeSun(data.StarData.Radius, data.StarData.Density, data.StarData.InnerGravityStrength, data.StarData.OuterGravityStrength, data.StarData.Type);            
            _spaceManager.CreateBorderAndSunGravity(data.AreaSize);
            
            _targetingService.DisableTargetSetting();

            #region Planet Data

            foreach(PlanetData_SystemView p in data.Planets)
            {
                _spaceManager.CreatePlanet(p.Distance, p.MaxTrip, p.PlanetType, p.CurrentTrip, p.Scale, p.Id, data.Id, false);
                _physicsManager.World.ProcessChanges();
            }

           
            #endregion
            
            #region Moon Data

            foreach (MoonData_SystemView p in data.Moons)
            {
                _spaceManager.CreatePlanet(p.Distance, p.MaxTrip, p.PlanetType, p.CurrentTrip, p.Scale, p.Id, p.IDToOrbit, true);
                _physicsManager.World.ProcessChanges();
            }
            #endregion

            #region Warphole Data

            foreach(WarpholeData w in data.Warpholes)
            {
                _spaceManager.CreateWarphole(w.XPos, w.YPos, w.WarpIndex, w.DestinationAreaID);
            }        

            #endregion

            #region Structure Data

            foreach(StructureData s in data.Structures)
            {
                var structure = InstantiateStructure(s);
            }

            #endregion

            #region Port Data
            foreach (PortData_SystemView p in data.Ports)
            {
                _spaceManager.CreatePort(p.Distance, p.MaxTrip, PlanetTypes.Port, p.CurrentTrip, 1, p.Id, data.Id, p.IsMoon);
            }

            #endregion

            _floatyAreaObjectManager.InstantiateFloatyAreaObjects(data.FloatyAreaObjects);
            if (_clientShipManager.PlayerShip != null)
            {
                _clientShipManager.RecreatePlayerShip(_physicsManager.World);
                _clientShipManager.PlayerShip.Position = new Vector2(data.NewPlayerXPos, data.NewPlayerYPos);
            }
            _clientShipManager.areaSecurityLevel = data.SecurityLevel;

            foreach (var s in data.Ships)
            {
                MessageReader.InstantiateShip(s, _physicsManager.World, _clientShipManager, false);
            }

            _targetingService.EnableTargetSetting();

            _areaDataReceived = true;

            return data;
        }

        public void SetAreaId(int? areaId)
        {
            AreaID = areaId;
        }
        
    }
}