using Freecon.Client.Core.Factories;
using Freecon.Client.Core.Interfaces;
using Freecon.Client.Core.Interfaces.States;
using Freecon.Client.Core.States;
using Freecon.Client.Core.States.Components;
using Freecon.Client.ViewModel;
using Freecon.Core.Networking.Models;
using Freecon.Models.TypeEnums;
using Microsoft.Xna.Framework;
using Freecon.Client.Interfaces;
using Freecon.Client.Managers;
using Freecon.Client.Objects;
using Freecon.Client.Objects.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using Freecon.Client.Core.Extensions;
using Freecon.Client.Objects.Pilots;
using Freecon.Client.Objects.Weapons;
using Freecon.Client.Core.Managers;
using Freecon.Models;
using Freecon.Core.Networking.Models.Objects;
using Freecon.Core.Networking.Models.Messages;
using Freecon.Core.Networking.Interfaces;
using Core.Networking;
using Freecon.Client.Managers.Networking;
using Freecon.Client.View;
using Freecon.Client.View.CefSharp;
using Freecon.Core.Utils;
using Freecon.Client.View.CefSharp.States;
using Server.Managers;

namespace Freecon.Client.GameStates
{
    public abstract class PlayableGameState : ActivePlayerGameState
    {

        protected readonly CollisionManager _collisionManager;
        protected readonly PhysicsManager _physicsManager;
        protected readonly ProjectileManager _projectileManager;
        protected readonly SelectionManager _selectionManager;
        protected readonly ClientShipManager _clientShipManager;
        protected readonly SimulationManager _simulationManager;
        protected readonly StructureFactoryManager _structureFactory;
        protected readonly WarpHoleManager _warpholeManager;
        protected readonly TeamManager _teamManager;
        protected readonly TargetingService _targetingService;
        protected readonly FloatyAreaObjectManager _floatyAreaObjectManager;

        /// <summary>
        /// Lets the simulator know when a PlayerPilot has entered the area when it previously had none. int is areaID
        /// </summary>
        public EventHandler<int> IncreaseUpdateInterval;

        /// <summary>
        /// Lets the simulator know when the last PlayerPilot has left the area. int is areaID
        /// </summary>
        public EventHandler<int> DecreaseUpdateInterval;

        private int _numPlayerPilots;

        /// <summary>
        /// Tracks whether the state manager has received the current state data from the server (e.g. system data)
        /// </summary>
        protected bool _areaDataReceived { get; set; }

        public IStructureManager StructureManager { get; protected set; }

        public PlayableGameState(MessageHandlerID messageHandlerId, IClientPlayerInfoManager clientPlayerInfoManager, 
            INetworkingService networkingService,
            CollisionManager collisionManager,
            PhysicsManager physicsManager,
            PlayerShipManager playerShipManager,
            ProjectileManager projectileManager,
            SelectionManager selectionManager,
            ClientShipManager clientShipManager,
            SimulationManager simulationManager,
            StructureFactoryManager structureFactoryManager,
            WarpHoleManager warpholeManager,
            TeamManager teamManager,
            TargetingService targetingService,
            FloatyAreaObjectManager floatyAreaObjectManager,
            MessageService_ToServer messageService,
            IStructureManager structureManager,
            GameStateType stateType
            )
            : base(messageHandlerId, clientPlayerInfoManager, networkingService, messageService, playerShipManager, stateType)
        {
            _collisionManager = collisionManager;
            _physicsManager = physicsManager;
            _projectileManager = projectileManager;
            _selectionManager = selectionManager;
            _clientShipManager = clientShipManager;
            _simulationManager = simulationManager;
            _structureFactory = structureFactoryManager;
            _warpholeManager = warpholeManager;
            _teamManager = teamManager;
            _targetingService = targetingService;
            _floatyAreaObjectManager = floatyAreaObjectManager;
            StructureManager = structureManager;

            _networkingService.RegisterMessageHandler(this, _playableStateManager_MessageReceived);

        }

        protected virtual void _playableStateManager_MessageReceived(object sender, NetworkMessageContainer e)
        {
            if (Status == GameStateStatus.Active || Status == GameStateStatus.Activating)
            {
                if (_checkActiveMessages(e.MessageData, e.MessageType))
                {
                    return;
                }
            }

            switch (e.MessageType)
            {

                #region Receieve Login Information (State, etc)

                case (MessageTypes.ClientLoginSuccess): //Login Successful
                    {
                        _targetingService.DisableTargetSetting();//Suspended until gamestate is read, where it should be enabled

                        var data = e.MessageData as MessageClientLogin;

                        _clientShipManager.RemoveAllShips();
                        _clientShipManager.CurrentCash = data.CurrentCash;
                        _clientPlayerInfoManager.PlayerID = data.PlayerInfo.PlayerID;

                        MessageReader.InstantiateShip(data.Ship, _physicsManager.World, _clientShipManager, true);
                       

                        string incomingMessage = data.LoginMessage;

                        return;
                    }
                #endregion
            
            }
        }

        /// <summary>
        /// Checks messages which should only be handled if the state is active. Returns true if a message is processed.
        /// </summary>
        /// <param name="?"></param>
        /// <returns>Returns true if a message is processed.</returns>
        bool _checkActiveMessages(MessagePackSerializableObject msg, MessageTypes messageType)
        {

            // All message cases must return true to signify that a message has been processed.

            switch (messageType)
            {
                #region Change ship location

                case (MessageTypes.ChangeShipLocation):
                    {
                        var data = msg as MessageChangeShipLocation;

                        Ship s = _clientShipManager.GetShip(data.ShipID);

                        if (s == null)
                        {
                            ConsoleManager.WriteLine("Warning: ShipID not found while processing ChangeShipLocation message.", ConsoleMessageType.Warning);
                            break;
                        }
                        s.Position = new Vector2(data.PosX, data.PosY);
                        s.LinearVelocity = new Vector2(data.VelX, data.VelY);
                        s.Rotation = data.Rotation;

                        return true;
                    }

                #endregion

                #region Receieve Position Update

                case (MessageTypes.PositionUpdateData):
                {
                    var data = msg as MessagePositionUpdateData;

                    if (data == null)
                    {
                        return true;
                    }

                    data.UpdateDataObjects
                        .Where(p => p.TargetType == PositionUpdateTargetType.Ship)
                        .ForEach(UpdateShipPosition);

                    
                    return true;
                }

                #endregion

                #region Add/Remove Ships to/from Team

                case (MessageTypes.AddRemoveShipsTeam):
                    {
                        MessageAddRemoveShipsTeam data = msg as MessageAddRemoveShipsTeam;

                        _targetingService.DisableTargetSetting();
                        if (data.AddOrRemove)
                        {
                            foreach (var ID in data.IDs)
                            {
                                _teamManager.AddTeamToObject(ID, data.TeamID);
                            }
                        }
                        else
                        {
                            foreach (var ID in data.IDs)
                            {
                                _teamManager.RemoveTeamFromObject(ID, data.TeamID);
                            }
                        }
                        _targetingService.EnableTargetSetting();
                        return true;
                    }

                #endregion

                #region Receive New Ship

                // New connection, got a player ID
                case (MessageTypes.ReceiveNewShips):
                    {
                        var data = msg as MessageReceiveNewShips;
                        foreach (var sd in data.Ships)
                        {
                            var s = MessageReader.InstantiateShip(sd, _physicsManager.World, _clientShipManager, false);

                            if (s.Pilot.PilotType == PilotType.Player)
                            {
                                _numPlayerPilots++;
                                if (_numPlayerPilots == 1 && IncreaseUpdateInterval != null) //Added the first player pilot
                                {
                                    IncreaseUpdateInterval(this, MessageHandlerID.Value);
                                }
                            }
                        
                        }
                        return true;
                    }
                #endregion

                #region NumOnlinePlayersChanged (Redis only)
                case MessageTypes.Redis_NumOnlinePlayersChanged:
                {
                    var data = msg as MessageEmptyMessage;
                    if (data.Data != null)
                    {
                        _numPlayerPilots = (int) data.Data;
                        if (_numPlayerPilots >= 1 && IncreaseUpdateInterval != null) //Added the first player pilot
                        {
                            IncreaseUpdateInterval(this, MessageHandlerID.Value);
                        }
                        else if (_numPlayerPilots == 0 && DecreaseUpdateInterval != null)
                        {
                            DecreaseUpdateInterval(this, MessageHandlerID.Value);
                        }
                    }
                    return true;
                }

                #endregion

                #region Receive New Structure
                case MessageTypes.ReceiveNewStructure:
                    {
                        var data = msg as MessageReceiveNewStructure;
                        StructureData sd = data.StructureData;
                        InstantiateStructure(sd);

                        return true;
                    }
                #endregion

                #region ReceiveFloatyAreaObjects

                case MessageTypes.ReceiveFloatyAreaObjects:
                    {
                        var data = msg as MessageReceiveFloatyAreaObjects;
                        _floatyAreaObjectManager.InstantiateFloatyAreaObjects(data);
                        break;
                    }

                #endregion

                #region KillOrRemoveObjectsFromArea
                case MessageTypes.RemoveKillRevive:
                    {
                        MessageRemoveKillRevive data = msg as MessageRemoveKillRevive;

                        switch (data.ObjectType)
                        {
                            case RemovableObjectTypes.FloatyAreaObject:
                                {
                                    _floatyAreaObjectManager.RemoveFloatyAreaObjects(data);
                                    break;
                                }
                            case RemovableObjectTypes.Ship:
                                {
                                    List<Ship> ships = new List<Ship>();
                                    foreach (var ID in data.ObjectIDs)
                                    {
                                        Ship s = _clientShipManager.GetShip(ID);
                                        if (s == null)
                                        {
                                            ConsoleManager.WriteLine("Error: ShipID not found in MainNetworkingManager while processing MessageTypes.RemoveKillRevive", ConsoleMessageType.Error);
                                        }
                                        else
                                        {
                                            ships.Add(s);
                                        }
                                    }


                                    switch (data.ActionType)
                                    {
                                        case ActionType.Kill:
                                            {
                                                foreach (Ship s in ships)
                                                {
                                                    s.Kill();
                                                    if (s != _clientShipManager.PlayerShip)
                                                    {
                                                        Debugging.DisposeStack.Push(this.ToString());
                                                        s.Body.Enabled = false;
                                                    }
                                                    _targetingService.DeRegisterObject(s);

                                                }
                                                break;
                                            }
                                        case ActionType.Revive:
                                            {
                                                float healthMultiplier = data.HealthMultiplier == null ? 1 : (float)data.HealthMultiplier;
                                                float shieldsMultiplier = data.ShieldMultiplier == null ? 1 : (float)data.ShieldMultiplier;
                                                foreach (Ship s in ships)
                                                {
                                                    s.Revive((int)(s.MaxHealth * healthMultiplier), (int)(s.MaxShields * shieldsMultiplier));
                                                    _targetingService.RegisterObject(s);
                                                }

                                                break;
                                            }

                                        case ActionType.Remove:
                                            {
                                                foreach (Ship s in ships)
                                                {
                                                    _clientShipManager.RemoveShip(s.Id);
                                                    if (s.Pilot.PilotType == PilotType.Player)
                                                    {
                                                        _numPlayerPilots--;
                                                        if (_numPlayerPilots == 0 && DecreaseUpdateInterval != null)
                                                            //Last player pilot has been removed
                                                        {
                                                            DecreaseUpdateInterval(this, MessageHandlerID.Value);
                                                        }
                                                    }
                                                
                                                }
                                                break;
                                            }
                                    }

                                    break;
                                }


                            case RemovableObjectTypes.Structure:
                                {
                                    switch (data.ActionType)
                                    {
                                        case ActionType.Kill:
                                            {
                                                foreach (var ID in data.ObjectIDs)
                                                {
                                                    KillStructure(ID);
                                                }
                                                break;
                                            }
                                        case ActionType.Revive:
                                            {
                                                ConsoleManager.WriteLine("Error: Revive not implemented for structures.", ConsoleMessageType.Error);
                                                break;
                                            }
                                        case ActionType.Remove:
                                            {
                                                foreach (var ID in data.ObjectIDs)
                                                {
                                                    RemoveStructure(ID);
                                                }
                                                break;
                                            }
                                    }

                                    break;


                                }

                        }
                        return true;
                    }

                #endregion

                #region ObjectFired

                case MessageTypes.ObjectFired:
                    {
                        var data = msg as MessageObjectFired;
                        ObjectFired(data);

                        return true;
                    }

                #endregion

                #region COMMAND Change Ship Type

                case (MessageTypes.ChangeShipType):
                    {
                        var data = msg as ShipData;
                        //This might not work...yolo
                        MessageReader.InstantiateShip(data, _physicsManager.World, _clientShipManager, true);
                        return true;
                    }
                #endregion

                #region Fire Denial

                case (MessageTypes.FireRequestResponse):
                    {
                        var data = msg as MessageFireRequestResponse;
                        switch (data.FiringObjectType)
                        {
                            case FiringObjectTypes.Ship:
                                {
                                   ShipFireResponse(data);
                                    break;
                                }


                            case FiringObjectTypes.Structure:
                                {
                                    StructureFireResponse(data);
                                    break;
                                }

                        }





                        return true;
                    }

                #endregion

                #region SelectorCommandMessage

                case MessageTypes.SelectorMessageType:
                    {
                        MessageSelectorCommand data = msg as MessageSelectorCommand;
                        _selectionManager.RelayNetworkCommand(data);
                        return true;
                    }
                #endregion

                #region Set Health

                case (MessageTypes.SetHealth):
                    {
                        var setHealthData = msg as MessageSetHealth;
                        foreach (var d in setHealthData.HealthData)
                        {
                            Ship s = _clientShipManager.GetShip(d.ShipID);

                            if (s == null)
                            {
                                ConsoleManager.WriteLine("Warning: ShipID not found while processing SetHealth message.", ConsoleMessageType.Warning);
                                break;
                            }

                            if (!s.IsAlive)
                                s.Revive((int)d.Health, (int)d.Shields);

                            s.CurrentHealth = d.Health;
                            s.Shields.CurrentShields = d.Shields;
                            s.SetCurrentEnergy(d.Energy);

                            for (int i = 0; i < d.DebuffTypesToAdd.Count; i++)
                            {
                                s.Debuffs.AddDebuff(d.DebuffTypesToAdd[i], d.DebuffCountsToAdd[i], TimeKeeper.MsSinceInitialization);
                            }

                        }

                        return true;
                    }
                #endregion

                #region AddCargoToShip
                case MessageTypes.AddCargoToShip:
                    {
                        var data = msg as MessageAddCargoToShip;

                        Ship s = _clientShipManager.GetShip(data.ShipID);
                        if (s == null)
                        {
                            ConsoleManager.WriteLine("Warning: ShipID not found while processing AddStatefulCargoToShip message.", ConsoleMessageType.Warning);
                            break;
                        }

                        foreach (var cargoData in data.StatefulCargoData)
                        {
                            var cargo = MessageReader.InstantiateStatefulCargo(cargoData);
                            
                            s.Cargo.AddStatefulCargo(cargo, true);
                            if (cargo.CargoType == StatefulCargoTypes.Module)
                            {
                                s.AddModule((Module)cargo);
                            }
                        }

                        foreach(var cargoData in data.StatelessCargoData)
                        {
                            s.Cargo.AddStatelessCargo(cargoData.CargoType, cargoData.Quantity, true);
                        }

                        break;
                    }

                #endregion

                #region RemoveCargoFromPlayerShip
                case MessageTypes.RemoveCargoFromShip:
                {
                    var data = msg as MessageRemoveCargoFromShip;

                    Ship s = _clientShipManager.GetShip(data.ShipID);

                    if (s == null)
                    {
                        ConsoleManager.WriteLine("Warning: ShipID not found while processing RemoveStatefulCargoFromShip message.", ConsoleMessageType.Warning);
                        break;
                    }

                    foreach (var cargoID in data.StatefulCargoIDs)
                    {
                        var removedCargo = s.Cargo.RemoveStatefulCargo(cargoID);

                        if (removedCargo == null)
                        {
                            ConsoleManager.WriteLine("Error: cargo not found while processing message " + MessageTypes.RemoveCargoFromShip + " for ID " + s.Id, ConsoleMessageType.Error);
                        }
                        else if (removedCargo.CargoType == StatefulCargoTypes.Module)
                        {
                            s.RemoveModule(removedCargo.Id);
                        }


                        foreach (var cargoData in data.StatelessCargoData)
                        {
                            s.Cargo.RemoveStatelessCargo(cargoData.CargoType, cargoData.Quantity);
                        }
                    }
                    break;
                }

                    #endregion

                


            }

            return false;//No message processed

        }

        private void UpdateShipPosition(PositionUpdateData posUpdate)
        {
            var s = _clientShipManager.GetShip(posUpdate.TargetId);

            if (s == null)
            {
                ClientLogger.Log(Log_Type.ERROR, "RECIEVED POSITION UPDATE FOR A SHIP THAT WAS NOT IN THE SHIPLIST");
                return;
            }
#if DEBUG
            if (_clientShipManager.PlayerShip != null && s.Id == _clientShipManager.PlayerShip.Id)
            {
                ConsoleManager.WriteLine("Error: Received position update for player ship!", ConsoleMessageType.Error);
            }
#endif

            _clientShipManager.HandlePositionUpdate(posUpdate);

            s.Shields.CurrentShields = (int)posUpdate.CurrentShields;
            s.CurrentHealth = (int)posUpdate.CurrentHealth;
            s.Thrusting = posUpdate.Thrusting;
        }

        public override void Activate(IGameState previous)
        {
            if (!_areaDataReceived || _clientShipManager.PlayerShip == null)
            {
                Status = GameStateStatus.Activating;
            }
            else
            {
                Status = GameStateStatus.Active;
                _clientShipManager.SendPositionUpdates = true;
            }
        }

        public override void Deactivate(IGameState next)
        {
            base.Deactivate(next);
            _areaDataReceived = false;
            _clientShipManager.SendPositionUpdates = false;
        }

        public virtual void AddStructure(Structure s)
        {
            if (s == null)
            {
                return;
            }

            StructureManager.AddStructure(s);
        }

        public virtual Structure InstantiateStructure(StructureData structureData)
        {
            Structure structure = _structureFactory.CreateStructure(structureData);

            if (structure != null)
            {
                _targetingService.RegisterObject(structure);
                AddStructure(structure);
                if (structure.IsLocalSim)
                    _simulationManager.StartSimulating(structure);
            }

            return structure;
        }


        /// <summary>
        /// Returns a reference to the structure with the passed ID if it exists, null otherwise
        /// </summary>
        /// <param name="structureID"></param>
        /// <returns></returns>
        public Structure GetStructureByID(int structureID)
        {
            return StructureManager.Structures.FirstOrDefault(p => p.Id == structureID);
        }

        public virtual void KillStructure(int ID)
        {
            var structure = GetStructureByID(ID);

            if (structure == null)
            {
                return;
            }

            structure.Kill();
                     

            RemoveStructure(structure);
        }

        public virtual void RemoveStructure(int ID)
        {
            var structure = GetStructureByID(ID);

            if (structure == null)
            {
                return;
            }

            RemoveStructure(structure);

        }

        public virtual void RemoveStructure(Structure structure)
        {
            if (structure is ITargetable)
            {

                foreach (var kvp in StructureManager.Structures)
                {
                    if (kvp is Turret)
                    {
                        ((Turret)kvp).PotentialTargets.Remove(structure.Id);
                    }
                }

                _targetingService.DeRegisterObject(structure);

                if (structure is ITeamable)
                {
                    _teamManager.DeRegisterObject((ITeamable)structure);
                }
            }

            StructureManager.RemoveStructure(structure);

        }

        public override void Clear()
        {
            _collisionManager.Reset();
            if (_clientShipManager.PlayerShip != null)
                _physicsManager.Reset(_clientShipManager.PlayerShip.Body);

            _projectileManager.Clear();
            _selectionManager.Clear();
            _clientShipManager.Reset();
            _simulationManager.Clear();
            _warpholeManager.Clear();
            _teamManager.Clear();
            StructureManager.Clear();
            _floatyAreaObjectManager.Clear();

        }

        public void ShipFireResponse(MessageFireRequestResponse data)
        {
            var firingShip = _clientShipManager.GetShip(data.FiringObjectID);

            if (firingShip == null)
            {
                ConsoleManager.WriteLine("Warning: ship not found on fire request response.", ConsoleMessageType.Warning);
                return;
            }
           
            firingShip.WaitingForFireResponse = false;
           
        }

        public void StructureFireResponse(MessageFireRequestResponse data)
        {
            var structure = GetStructureByID(data.FiringObjectID);

            if (structure != null) // Denial may be received after kill message
            {
                structure.WaitingForFireResponse = false;
            }
        }

        public void ObjectFired(MessageObjectFired data)
        {
            switch (data.ObjectType)
            {
                case FiringObjectTypes.Structure:
                    {
                        var structure = GetStructureByID(data.FiringObjectID);

                        if (structure != null) // In case we get a fire message after a kill
                        {

                            if (structure.Weapon.Stats.NumProjectiles != data.ProjectileIDs.Count)
                            {
                                ConsoleManager.WriteLine("Error: MessageObjectFired received with different number of projectileIDs than expected. Weapons for this object are not synced between server and client.", ConsoleMessageType.Error);
                                return;
                            }

                            structure.Weapon.Fire_ServerOrigin(data.ProjectileIDs, structure.Rotation, data.PercentCharge, true);

                            if (structure.StructureType == StructureTypes.DefensiveMine)
                            {
                                structure.Kill();
                                RemoveStructure(structure);
                            }

                        }
                        break;
                    }
                case FiringObjectTypes.Ship:
                    {
                        Ship tempShip = _clientShipManager.GetShip(data.FiringObjectID);

                        if (tempShip != null)
                        {
                            Weapon w = tempShip.GetWeapon(data.WeaponSlot);

                            if (w != null)
                            {
                                if (w.Stats.NumProjectiles != data.ProjectileIDs.Count)
                                {
                                    ConsoleManager.WriteLine("Error: MessageObjectFired received with different number of projectileIDs than expected. Weapons for this object are not synced between server and client.", ConsoleMessageType.Error);
                                    return;
                                }

                                w.Fire_ServerOrigin(data.ProjectileIDs, data.Rotation, data.PercentCharge, true);
                            }

                        }
                        break;
                    }

            }


        }

        public virtual void Dispose()
        {
            

        }

        ~PlayableGameState()
        {
            _networkingService.DeregisterMessageHandler(this, _playableStateManager_MessageReceived);
        }

        public override UIStateManagerContainer GetStateManagerContainer()
        {
            return new PlayableUIStateManagerContainer(StateType, _playerShipManager, _clientPlayerInfoManager);
        }

        /// <summary>
        /// Needs to be moved
        /// </summary>
        public class NetworkStructureModel
        {
            public StructureTypes StructureType { get; set; }
            public Vector2 Position { get; set; }
            public float CurrentHealth { get; set; }
            public int ID { get; set; }
            public bool IsLocalSim { get; set; }

            public NetworkStructureModel()
            { }

            public HashSet<int> Teams { get; set; }

            public NetworkStructureModel(NetworkStructureModel m)
            {
                StructureType = m.StructureType;
                Position = m.Position;
                CurrentHealth = m.CurrentHealth;
                ID = m.ID;
                Teams = new HashSet<int>();
            }
        }
        /// <summary>
        /// Needs to be moved
        /// </summary>
        public class NetworkTurretModel : NetworkStructureModel
        {
            public TurretTypes TurretType { get; set; }

            public NetworkTurretModel()
            {
            }

            public NetworkTurretModel(NetworkStructureModel m)
                : base(m)
            {
            }
        }

        
    }

    public interface IWebGameState<TViewModel, TWebView> : IHasWebGameState
        where TViewModel : IViewModel
        where TWebView : IGameWebView<TViewModel>
    {
        TWebView WebView { get; }
    }

    /// <summary>
    /// Allows us to fetch the non-generic GameWebView and call it's methods.
    /// </summary>
    public interface IHasWebGameState
    {
        IHasGameWebView RawGameWebView { get; }
    }

    public abstract class PlayableGameStateClient<TViewModel, TWebView> : PlayableGameState, IPlayableGameState, IWebGameState<TViewModel, TWebView>
        where TViewModel : PlayableViewModel
        where TWebView : IGameWebView<TViewModel>
    {

        public int? AreaID;//Probably temporary.

        public TViewModel ViewModel { get; protected set; }
        public TWebView WebView { get; protected set; }
        public IHasGameWebView RawGameWebView => WebView;


        public PlayableGameStateClient(MessageHandlerID messageHandlerId,
            IClientPlayerInfoManager clientPlayerInfoManager,
            CollisionManager collisionManager,
            IGlobalGameUISingleton globalGameUiSingleton,
            INetworkingService networkingService,
            PhysicsManager physicsManager,
            PlayerShipManager playerShipManager,
            ProjectileManager projectileManager,
            SelectionManager selectionManager,
            ClientShipManager clientShipManager,
            SimulationManager simulationManager,
            StructureFactoryManager structureFactoryManager,
            WarpHoleManager warpholeManager,
            TeamManager teamManager,
            TargetingService targetingService,
            FloatyAreaObjectManager floatyAreaObjectManager,
            MessageService_ToServer messageService,
            GameStateType stateType,
            TViewModel viewModel
            )
            : base(messageHandlerId, clientPlayerInfoManager, networkingService, collisionManager, physicsManager, playerShipManager, projectileManager, selectionManager, clientShipManager, simulationManager, structureFactoryManager, warpholeManager, teamManager, targetingService, floatyAreaObjectManager, messageService, viewModel, stateType)
        {
            ViewModel = viewModel;
            WebView = (TWebView)Activator.CreateInstance(typeof(TWebView), globalGameUiSingleton, ViewModel);
            SetupUpdateList(_synchronousUpdateList);
        }



        public virtual void SetupUpdateList(HashSet<ISynchronousUpdate> updateList)
        {          
            updateList.Add(_physicsManager);
            updateList.Add(_warpholeManager);
            updateList.Add(_projectileManager);
            updateList.Add(_clientShipManager);
            updateList.Add(_simulationManager);
            updateList.Add(_selectionManager);
            updateList.Add(_collisionManager);
            updateList.Add(_projectileManager);
            updateList.Add(_floatyAreaObjectManager);
            updateList.Add(StructureManager);

            if (updateList.Contains(null))
                updateList.Remove(null);//Gross

        }                

        public override void Activate(IGameState previous)
        {
            //Debug
            Debugging.playerShipManager = _playerShipManager;

            if (!_areaDataReceived || _clientShipManager.PlayerShip == null)
            {
                Status = GameStateStatus.Activating;
            }
            else
            {
                Status = GameStateStatus.Active;
                _clientShipManager.SendPositionUpdates = true;
            }
        }

        public override void Deactivate(IGameState next)
        {
            base.Deactivate(next);
            _areaDataReceived = false;
            _clientShipManager.SendPositionUpdates = false;
        }  
        

        public override void Clear()
        {
            _collisionManager.Reset();
            if(_clientShipManager.PlayerShip != null)
                _physicsManager.Reset(_clientShipManager.PlayerShip.Body);

            _projectileManager.Clear();
            
            if(_selectionManager != null)
                _selectionManager.Clear();

            _clientShipManager.Reset();
            _simulationManager.Clear();
            _warpholeManager.Clear();
            _teamManager.Clear();
            ViewModel.Clear();
            _floatyAreaObjectManager.Clear();

        }
    }
    /// <summary>
    /// Needs to be moved
    /// </summary>
    public class NetworkStructureModel
    {
        public StructureTypes StructureType { get; set; }
        public Vector2 Position { get; set; }
        public float CurrentHealth { get; set; }
        public int ID { get; set; }
        public bool IsLocalSim { get; set; }

        public NetworkStructureModel()
        { }

        public HashSet<int> Teams { get; set; }

        public NetworkStructureModel(NetworkStructureModel m)
        {
            StructureType = m.StructureType;
            Position = m.Position;
            CurrentHealth = m.CurrentHealth;
            ID = m.ID;
            Teams = new HashSet<int>();
        }
    }
    /// <summary>
    /// Needs to be moved
    /// </summary>
    public class NetworkTurretModel : NetworkStructureModel
    {
        public TurretTypes TurretType { get; set; }

        public NetworkTurretModel()
        {
        }

        public NetworkTurretModel(NetworkStructureModel m)
            : base(m)
        {
        }
    }
}
