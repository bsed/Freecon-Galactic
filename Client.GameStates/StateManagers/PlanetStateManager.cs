using System.Collections.Generic;
using Client.View.JSMarshalling;
using Microsoft.Xna.Framework;
using Freecon.Client.Objects;
using Freecon.Client.Managers.Networking;
using Freecon.Client.Objects.Structures;
using Freecon.Models.TypeEnums;
using Freecon.Client.Core.Factories;
using Freecon.Client.ViewModel;
using Freecon.Client.GameStates;
using Freecon.Client.Core.Interfaces;
using Freecon.Client.Core.States;
using Freecon.Core.Networking.Models;
using Freecon.Client.Core.Extensions;
using Freecon.Client.Core.States.Components;
using Freecon.Client.Core.Managers;
using Freecon.Core.Networking.Models.Objects;
using Freecon.Core.Networking.Models.Messages;
using Freecon.Core.Networking.Interfaces;
using Core.Networking;
using Freecon.Client.View.CefSharp;

namespace Freecon.Client.Managers.States
{
    public class PlanetStateManager :
        PlayableGameStateClient<PlanetViewModel, PlanetWebView>, IPlanetStateManager
    {

        MessageService_ToServer _messageService;

        protected int _wallWidth;
        protected int _wallHeight;

        public HashSet<int> ColonyTeamIDs {
            get { return ViewModel.ColonyTeamIDs; }
            set { ViewModel.ColonyTeamIDs = value; }
        }

        /// <summary>
        /// Manages all aspects of Planetary Invasion.
        /// </summary>
        /// <param name="Content">Content Manager</param>
        /// <param name="planetLevel">Level to play on.</param>
        public PlanetStateManager(MessageHandlerID messageHandlerId,
            IClientPlayerInfoManager clientPlayerInfoManager,
            IGlobalGameUISingleton globalGameUiSingleton,
            CollisionManager collisionManager,
            PhysicsManager physicsManager,
            PlayerShipManager playerShipManager,
            ProjectileManager projectileManager,
            ClientShipManager clientShipManager,
            StructureFactoryManager structureFactoryManager,
            WarpHoleManager warpholeManager,
            INetworkingService networkingService,
            SelectionManager selectionManager,
            SimulationManager simulationManager,
            TargetingService targetService,
            TeamManager teamManager,
            GameStateType stateType,//Should be moon or planet
            FloatyAreaObjectManager floatyAreaObjectManager,
            MessageService_ToServer messageService,
            int wallTexWidth, int wallTexHeight)
            : base(messageHandlerId,
                clientPlayerInfoManager,
                collisionManager,
                globalGameUiSingleton,
                networkingService,
                physicsManager,
                playerShipManager,
                projectileManager,
                selectionManager,
                clientShipManager,
                simulationManager,
                structureFactoryManager,
                warpholeManager,
                teamManager, 
                targetService,
                floatyAreaObjectManager,
                messageService,
                stateType, 
                new PlanetViewModel(clientShipManager)
            )
        {
            _wallWidth = wallTexWidth;
            _wallHeight = wallTexHeight;                
            _synchronousUpdateList.Add(_playerShipManager);
            _messageService = messageService;

        }



        protected override void _playableStateManager_MessageReceived(object sender, NetworkMessageContainer e)
        {
            base._playableStateManager_MessageReceived(sender, e);

            switch (e.MessageType)
            {                

                case MessageTypes.PlanetLandApproval:
                {
                    var data = e.MessageData as PlanetEntryData;
                    if (data == null)
                    {
                        ClientLogger.LogError("Could not deserialize PlanetEntryData: " + e.MessageData);
                        return;
                    }

                    ReadIncomingPlanet(data);

                    _targetingService.RegisterObject(_clientShipManager.PlayerShip);
                    break;
                }

                case MessageTypes.ColonyCaptured:
                {
                    var data = e.MessageData as MessageColonyCaptured;
                    if (data == null)
                    {
                        ClientLogger.LogError("Could not deserialize MessageColonyCaptured: " + e.MessageData);
                        return;
                    }

                    ColonyCaptured(data);
                    break;
                }

                case MessageTypes.ColonizeRequestApproval:
                {
                    var data = e.MessageData as MessageColonizeRequestApproval;
                    if (data == null)
                    {
                        ClientLogger.LogError("Could not deserialize MessageColonizeRequestApproval: " + e.MessageData);
                        return;
                    }

                    ColonizeRequestApproval(data);
                    break;
                }
            }
        }

        public override void Clear()
        {
            base.Clear();
            _targetingService.Clear(_clientShipManager.PlayerShip);
            ViewModel.ColonyTeamIDs.Clear();
        }

        public override void Activate(IGameState previous)
        {
            base.Activate(previous);
        }

        protected void ColonizeRequestApproval(MessageColonizeRequestApproval data)
        {
            
            
            data.ColonyStructure.OwnerTeamIDs = ColonyTeamIDs;
            InstantiateStructure(data.ColonyStructure);
            

            _clientShipManager.PlayerShip.Cargo.RemoveStatelessCargo(StatelessCargoTypes.Biodome, 1);
            if(data.ColonyStructure.OwnerDefaultTeamID != null)
                ColonyTeamIDs.Add((int)data.ColonyStructure.OwnerDefaultTeamID);
        
        }

        protected void ColonyCaptured(MessageColonyCaptured data)
        {            
            ColonyTeamIDs.Clear();
            ColonyTeamIDs.Add(data.OwnerTeamID);

            _targetingService.ResetTargeting();
        }

        public void CreateWarphole(float xpos, float ypos, byte warpIndex, int destinationAreaID)
        {
            _warpholeManager.CreateWarphole(xpos, ypos, warpIndex, destinationAreaID);
        }
                
        
        public void ReadIncomingPlanet(PlanetEntryData data)
        {         
            _targetingService.DisableTargetSetting();
            _clientShipManager.CurrentAreaId = data.Id;   

            
            if(data.PlanetTeamID != null)
            {
                ColonyTeamIDs.Add((int)data.PlanetTeamID);
            }



            #region Planet Layout


            var islands = new List<List<Vector2>>();


            //DEBUG
            //data.Layout.Layout1D = new bool[] { false, false, false, false, false, false,
            //                           false, true , true , true , true , false,
            //                           false, true , false, false, true , false,
            //                           false, true , false, false, true , false,
            //                           false, true , true , true , true , false,
            //                           false, false, false, false, false, false };
            //data.Layout.NumY = 6;
            //data.Layout.NumX = 6;


            LoadPlanetLevel(data.PlanetType, islands, data.Layout.NumX, data.Layout.NumY, data.Layout.Layout1D);
            #endregion

            #region Warphole Data

            foreach(WarpholeData w in data.Warpholes)
            {               
                CreateWarphole(w.XPos, w.YPos, w.WarpIndex, w.DestinationAreaID);
            }

            #endregion

            #region Structure Data

            foreach(StructureData d in data.Structures)
            {
                var structure = InstantiateStructure(d);                
            }

            #endregion


            if (_clientShipManager.PlayerShip != null)
            {
                if (!_clientShipManager.PlayerShip.IsBodyValid)//I don't know why the fuck this needs to happen sometimes, but occasionally the body will be disposed and setting the position will throw an exception
                    _clientShipManager.RecreatePlayerShip(_physicsManager.World);

                _clientShipManager.PlayerShip.Position = new Vector2(data.NewPlayerXPos, data.NewPlayerYPos);
                _clientShipManager.PlayerShip.LinearVelocity = Vector2.Zero;
            }
            // Alliance with planet owner
            _clientShipManager.areaSecurityLevel = data.SecurityLevel;
            

            foreach(ShipData s in data.Ships)
            {
                MessageReader.InstantiateShip(s, _physicsManager.World, _clientShipManager, false);
            }          
         
            _targetingService.EnableTargetSetting();
            _areaDataReceived = true;
        }

        public virtual void LoadPlanetLevel(
            PlanetTypes planetType,
            IEnumerable<IEnumerable<Vector2>> islands,
            int height,
            int width,
            bool[] layoutArray)
        {
            ViewModel.Level = new PlanetLevel(null, _wallWidth, _wallHeight, _physicsManager, planetType, islands, height, width, layoutArray);
        }

        public void LoadTestPlanet(PlanetTypes planetType)
        {
            bool[] layoutArray = new bool[] { false, false, false, false, false, false,
                                       false, true , true , true , true , false,
                                       false, true , false, false, true , false,
                                       false, true , false, false, true , false,
                                       false, true , true , true , true , false,
                                       false, false, false, false, false, false };

            // No walls
            //bool[] layoutArray = new bool[] {
            //                           false, false, false, false, false, false,
            //                           false, false , false, false, false, false,
            //                           false, false , false, false, false, false,
            //                           false, false , false, false, false, false,
            //                           false, false , false , false,false, false,
            //                           false, false, false, false, false, false };
            
            // Random islands, not currently implemented anyway
            List<List<Vector2>> islands = new List<List<Vector2>>();
            
            int height = 6;
            int width = 6;

            LoadPlanetLevel(planetType, islands, height, width, layoutArray);
                    

            var t = _structureFactory.CreateTurret(
                new Vector2(3, 3), 10000, 100, TurretTypes.Planet, new HashSet<int>());

            t.IsLocalSim = true;

            AddStructure(t);
#if DEBUG
            Debugging.SimulationManager.StartSimulating(t);
#endif
            //Ship npc = _clientShipManager.CreateShip(new Vector2(-1, -1), 7, 0, Vector2.Zero, "NPC", ShieldTypes.halo, ShipTypes.NPC_Penguin, WeaponTypes.Laser, WeaponTypes.Laser, _clientShipManager.PlayerShip.Teams, false);
            
            //npc.IsLocalSim = true;
            //Debugging.SimulationManager.StartSimulating(npc);
            _clientShipManager.PlayerShip.Cargo.AddStatelessCargo(StatelessCargoTypes.AmbassadorMissile, 999, true);

        }

        public void PlanetStateManager_OnStructureRemoved(object sender, StructureRemovedEventArgs e)
        {
            if (e.RemovedStructure != null && e.RemovedStructure is Turret)
            {
                ViewModel.RemoveTurret(e.RemovedStructure as Turret);
            }
        }

        protected void SendStructurePlacementRequest(JSMarshallContainer request)
        {
            var req = request as StructurePlacementRequest;//Hmmm...would be nice to come up with a way to avoid this.
            _messageService.SendStructurePlacementRequest(req.StructureType, new Vector2(req.PosX, req.PosY), _playerShipManager.PlayerShip.Id);
        }

        public void SetAreaId(int? areaId)
        {
            AreaID = areaId;
        }
    }
}