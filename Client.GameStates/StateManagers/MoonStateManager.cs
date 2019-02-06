//using Core.Networking;
//using Freecon.Client.Core.Extensions;
//using Freecon.Client.Core.Factories;
//using Freecon.Client.Core.Managers;
//using Freecon.Client.Core.States;
//using Freecon.Core.Networking.Interfaces;
//using Freecon.Core.Networking.Models;
//using Freecon.Core.Networking.Models.Objects;
//using Microsoft.Xna.Framework;
//using System.Collections.Generic;
//using Freecon.Client.View.CefSharp;

//namespace Freecon.Client.Managers.States
//{
//    public class MoonStateManager:PlanetStateManager
//    {
//        /// <summary>
//        /// For now, just derives from DrawableMoonStateManager. We can override stuff if/as necessary later. Maybe we don't want a moon state?
//        /// </summary>
//        public MoonStateManager(MessageHandlerID messageHandlerId,
//            IClientPlayerInfoManager clientPlayerInfoManager,
//            CollisionManager collisionManager,
//            IGlobalGameUISingleton globalGameUiSingleton,
//            PhysicsManager physicsManager,
//            PlayerShipManager playerShipManager,
//            ProjectileManager projectileManager,
//            ClientShipManager clientShipManager,
//            StructureFactoryManager structureFactoryManager,
//            WarpHoleManager warpholeManager,
//            INetworkingService networkingService,
//            SelectionManager selectionManager,
//            SimulationManager simulationManager,
//            TargetingService targetService,
//            TeamManager teamManager,
//            FloatyAreaObjectManager floatyAreaObjectManager,
//            int wallTexWidth, int wallTexHeight):base(messageHandlerId,
//            clientPlayerInfoManager,
//            globalGameUiSingleton,
//            collisionManager,
//            physicsManager,
//            playerShipManager,
//            projectileManager,
//            clientShipManager,
//            structureFactoryManager,
//            warpholeManager,
//            networkingService,
//            selectionManager,
//            simulationManager,
//            targetService,
//            teamManager,
//            GameStateType.Moon,
//            floatyAreaObjectManager,
//            wallTexWidth, wallTexHeight)
//        {
            
//        }


//        protected override void _playableStateManager_MessageReceived(object sender, NetworkMessageContainer e)
//        {
//            base._playableStateManager_MessageReceived(sender, e);

//            switch (e.MessageType)
//            {

//                case MessageTypes.MoonLandApproval:
//                    {

//                        var data = e.MessageData as MoonEntryData;
//                        ReadIncomingMoon(data);

//                        _targetingService.RegisterObject(_clientShipManager.PlayerShip);
//                        break;
//                    }
                
//            }
//        }

//        public void ReadIncomingMoon(MoonEntryData data)
//        {
//            _targetingService.DisableTargetSetting();
//            _clientShipManager.CurrentAreaId = data.Id;


//            if (data.PlanetTeamID != null)
//            {
//                ColonyTeamIDs.Add((int)data.PlanetTeamID);
//            }



//            #region Planet Layout


//            var islands = new List<List<Vector2>>();


//            //DEBUG
//            //data.Layout.Layout1D = new bool[] { false, false, false, false, false, false,
//            //                           false, true , true , true , true , false,
//            //                           false, true , false, false, true , false,
//            //                           false, true , false, false, true , false,
//            //                           false, true , true , true , true , false,
//            //                           false, false, false, false, false, false };
//            //data.Layout.NumY = 6;
//            //data.Layout.NumX = 6;


//            LoadPlanetLevel(data.PlanetType, islands, data.Layout.NumX, data.Layout.NumY, data.Layout.Layout1D);
//            #endregion

//            #region Warphole Data

//            foreach (WarpholeData w in data.Warpholes)
//            {
//                CreateWarphole(w.XPos, w.YPos, w.WarpIndex, w.DestinationAreaID);
//            }

//            #endregion

//            #region Structure Data

//            foreach (StructureData d in data.Structures)
//            {
//                var structure = InstantiateStructure(d);
//            }

//            #endregion


//            if (_clientShipManager.PlayerShip != null)
//            {
//                if (!_clientShipManager.PlayerShip.IsBodyValid)//I don't know why the fuck this needs to happen sometimes, but occasionally the body will be disposed and setting the position will throw an exception
//                    _clientShipManager.RecreatePlayerShip(_physicsManager.World);

//                _clientShipManager.PlayerShip.Position = new Vector2(data.NewPlayerXPos, data.NewPlayerYPos);
//                _clientShipManager.PlayerShip.LinearVelocity = Vector2.Zero;
//            }
//            // Alliance with planet owner
//            _clientShipManager.areaSecurityLevel = data.SecurityLevel;


//            foreach (ShipData s in data.Ships)
//            {
//                MessageReader.InstantiateShip(s, _physicsManager.World, _clientShipManager, false);
//            }

//            _targetingService.EnableTargetSetting();
//            _areaDataReceived = true;
//        }

//        public void SetAreaId(int? areaId)
//        {
//            AreaID = areaId;
//        }
//    }
//}
