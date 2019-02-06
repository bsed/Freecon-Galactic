using System;
using System.Collections.Generic;
using Freecon.Client.Core.Behaviors;
using Freecon.Client.Core.Objectss;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Core.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using Freecon.Client.Managers.Networking;
using Freecon.Client.Objects;
using Freecon.Client.Interfaces;
using Freecon.Client.Objects.Pilots;
using Freecon.Models.TypeEnums;
using Freecon.Client.Objects.Weapons;
using Core.Models;
using FarseerPhysics.Dynamics;
using Core.Models.Enums;
using Microsoft.Xna.Framework.Input;
using Freecon.Client.Core.Objects.Ships;
using Freecon.Core.Networking.Models.Objects;
using Server.Managers;
using Freecon.Client.Mathematics;
using Freecon.Core;

namespace Freecon.Client.Managers
{
    public class ClientShipManager : ISynchronousManager 
    {
        protected readonly ParticleManager _particleManager;
        protected readonly PlayerShipManager _playerShipManager;
        protected SpriteBatch _spriteBatch;
        protected TextureManager _textureManager;
        protected readonly SimulationManager _simulationManager;
        protected readonly TargetingService _targetManager;
        protected readonly TeamManager _teamManager;
        protected readonly ProjectileManager _projectileManager;
        protected MessageService_ToServer _messageService;
        protected readonly IClientPlayerInfoManager _clientPlayerInfoManager;

        protected float _positionUpdateInterval = 100;//TODO: Move to config file

        public float CurrentCash; //WARNING: Need to create a good place for this
        public byte areaSecurityLevel = 0; //255 Represents maximum security, weapons disabled

        /// <summary>
        /// This ship is also stored in the dictionary, to make updating easier.
        /// </summary>
        /// <value>
        /// The player ship.
        /// </value>
        public Ship PlayerShip { get { return _playerShipManager.PlayerShip; } }

        public Pilot PlayerPilot { get { return _playerShipManager.PlayerPilot; } }

        protected Dictionary<int, Ship> _shipList; // Contains all ships, including the player's ship

        public int CurrentAreaId { get; set; }
        
        /// <summary>
        /// Collection of ships for which to send update data
        /// </summary>
        protected HashSet<Ship> _positionUpdateList;  

        public bool SendPositionUpdates { get; set; }
        protected float _lastPositionUpdateTime { get; set; }

        /// <summary>
        /// If true, adds ships to the simulator.
        /// </summary>
        protected bool _simulateNPCs;

        /// <summary>
        /// Set of ids to ignore when server pushes updated. TODO: Check if this is necessary
        /// </summary>
        protected HashSet<int> _updateIgnoreList;

        #region GC Friendly Variables
        // GC Friendly
        private float LagDistance, RotationDistance;
        private float SpaceBetween;
        private Vector2 LagVelocity, IncomingPosition;
        private float SmoothStepX, SmoothStepY;
        private Vector2 IncomingVelocity;
        #endregion

        public ClientShipManager(
            ParticleManager particleManager,
            PlayerShipManager playerShipManager,
            SpriteBatch spriteBatch,
            TextureManager textureManager,
            SimulationManager simulationManager,
            TargetingService targetManager,
            TeamManager teamManager,
            ProjectileManager projectileManager,
            MessageService_ToServer messageService,
            IClientPlayerInfoManager clientPlayerInfoManager,
            bool simulateNPCs)
        {
            _shipList = new Dictionary<int, Ship>();
           
            _particleManager = particleManager;
            _playerShipManager = playerShipManager;
            _spriteBatch = spriteBatch;
            _textureManager = textureManager;
            _simulationManager = simulationManager;
            _targetManager = targetManager;
            _teamManager = teamManager;
            _messageService = messageService;
            _projectileManager = projectileManager;
            _updateIgnoreList = new HashSet<int>();
            _simulateNPCs = simulateNPCs;
            _positionUpdateList = new HashSet<Ship>();
            _clientPlayerInfoManager = clientPlayerInfoManager;
        }


        /// <summary>
        /// Updates ship based on server data
        /// </summary>
        public void HandlePositionUpdate(PositionUpdateData data)
        {
            if (!_shipList.ContainsKey(data.TargetId))
            {
                Console.WriteLine("Could not find ship. The new connection was never added to the shipList.");

                ClientLogger.Log(Log_Type.ERROR, "Could not find ship. The new connection was never added.");
                // Debug for Client

                return;
            }


            if (_updateIgnoreList.Contains(data.TargetId)) //Player's ship or locally simulated ships
            {
                ClientLogger.Log(Log_Type.WARNING, "Ignoring own player ID in update");
                return;
            }

            var ship = _shipList[data.TargetId];

            ship.UpdatePosition(data);
        }

        public void Update(IGameTimeService gameTime)
        {
            try
            {
                foreach (var kvp in _shipList)
                {
                    kvp.Value.Update(gameTime);
                }
                
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLine(e);

            }
            _checkKeys(gameTime);

            if (SendPositionUpdates && gameTime.TotalMilliseconds - _lastPositionUpdateTime > _positionUpdateInterval)
            {
                _sendPositionUpdates(gameTime);
            }
        }

        void _checkKeys(IGameTimeService gameTime)
        {

            if (PlayerPilot == null || PlayerPilot.PilotType != PilotType.Player)
            {
                return;//This is a semi ugly hack to enable bot clients for stress testing.
            }


            if (KeyboardManager.EnterMode.IsBindTapped() || GamepadManager.EnterMode.IsBindTapped())
            {
                if (PlayerShip.EnterMode == false)
                {
                    PlayerShip.EnterMode = true;
                }
                else
                {
                    PlayerShip.EnterMode = false;
                }
            }

#if ADMIN
            if (KeyboardManager.SetPosTo0.IsBindTapped())
            {
                PlayerShip.Position = new Microsoft.Xna.Framework.Vector2(0,0);
            }
            if(KeyboardManager.AdminMoveDown.IsBindPressed())
            {
                Vector2 oldPos = PlayerShip.Position;
                Vector2 newPos = new Vector2(oldPos.X, oldPos.Y + 1);
                PlayerShip.Position = newPos;
            }
            if (KeyboardManager.AdminMoveUp.IsBindPressed())
            {
                Vector2 oldPos = PlayerShip.Position;
                Vector2 newPos = new Vector2(oldPos.X, oldPos.Y - 1);
                PlayerShip.Position = newPos;
            }
            if (KeyboardManager.AdminMoveLeft.IsBindPressed())
            {
                Vector2 oldPos = PlayerShip.Position;
                Vector2 newPos = new Vector2(oldPos.X - 1, oldPos.Y);
                PlayerShip.Position = newPos;
            }
            if (KeyboardManager.AdminMoveRight.IsBindPressed())
            {
                Vector2 oldPos = PlayerShip.Position;
                Vector2 newPos = new Vector2(oldPos.X + 1, oldPos.Y);
                PlayerShip.Position = newPos;
            }
#endif


            Ship playerShip = PlayerShip;


            if (PlayerShip != null && PlayerShip.Pilot.IsAlive)
            {
                PlayerPilot playerPilot = (PlayerPilot) playerShip.Pilot;

                #region Movement

                if (KeyboardManager.HoldPlayerPosition.IsBindTapped() || GamepadManager.HoldPlayerPosition.IsBindTapped())
                    playerPilot.HoldingPosition = true;

                if (playerPilot.HoldingPosition)
                    playerPilot.StopShip(gameTime);

                if (KeyboardManager.TurnLeft.IsBindPressed() || GamepadManager.TurnLeft.IsBindPressed())
                {
                    playerShip.IsTurningCounterClockwise = true;
                    playerPilot.HoldingPosition = false;
                }
                else
                {
                    playerShip.IsTurningCounterClockwise = false;

                }
                if (KeyboardManager.TurnRight.IsBindPressed() || GamepadManager.TurnRight.IsBindPressed())
                {
                    playerShip.IsTurningClockwise = true;
                    playerPilot.HoldingPosition = false;
                }
                else
                    playerShip.IsTurningClockwise = false;

                if (KeyboardManager.ThrustUp.IsBindPressed() || GamepadManager.ThrustUp.IsBindPressed())
                {
                    if (KeyboardManager.Boost.IsBindPressed() || GamepadManager.Boost.IsBindPressed()) //If boosting
                        playerShip.Thrust(ThrustTypes.BoostForward);
                    else //If not boosting
                    {
                        playerShip.Thrust(ThrustTypes.Forward);
                    }
                    playerShip.Thrusting = true;
                    playerPilot.HoldingPosition = false;
                }
                else if (KeyboardManager.ThrustDown.IsBindPressed() || GamepadManager.ThrustDown.IsBindPressed())
                {
                    if (KeyboardManager.Boost.IsBindPressed() || GamepadManager.Boost.IsBindPressed())
                    {
                        playerShip.Thrust(ThrustTypes.BoostBackward);
                    }
                    else
                    {
                        playerShip.Thrust(ThrustTypes.Backward);
                    }
                    playerShip.Thrusting = true;
                    playerPilot.HoldingPosition = false;
                }
                else if (KeyboardManager.ThrustLateralLeft.IsBindPressed())
                {
                    if (KeyboardManager.Boost.IsBindPressed() || GamepadManager.Boost.IsBindPressed())
                    {
                        playerShip.Thrust(ThrustTypes.BoostLeftLateral);
                    }
                    else
                    {
                        playerShip.Thrust(ThrustTypes.LeftLateral);
                    }
                    playerShip.Thrusting = true;
                    playerPilot.HoldingPosition = false;
                }
                else if (KeyboardManager.ThrustLateralRight.IsBindPressed())
                {
                    if (KeyboardManager.Boost.IsBindPressed() || GamepadManager.Boost.IsBindPressed())
                    {
                        playerShip.Thrust(ThrustTypes.BoostRightLateral);
                    }
                    else
                    {
                        playerShip.Thrust(ThrustTypes.RightLateral);
                    }
                    playerShip.Thrusting = true;
                    playerPilot.HoldingPosition = false;
                }
                else
                {
                    playerShip.Thrusting = false;
                }

                #endregion

                #region Firing Weapons

                if (KeyboardManager.FireWeapon1.IsBindPressed() || GamepadManager.FirePrimary.IsBindPressed())
                    _weaponPressed(1, gameTime, PlayerShip, (PlayerPilot) PlayerShip.Pilot);
                else
                    _weaponNotPressed(1, gameTime, PlayerShip);

                if (KeyboardManager.FireWeapon2.IsBindPressed() || GamepadManager.FirePrimary.IsBindPressed())
                    _weaponPressed(2, gameTime, PlayerShip, (PlayerPilot) PlayerShip.Pilot);
                else
                    _weaponNotPressed(2, gameTime, PlayerShip);

                if (KeyboardManager.FireWeapon3.IsBindPressed() || GamepadManager.FirePrimary.IsBindPressed())
                    _weaponPressed(3, gameTime, PlayerShip, (PlayerPilot) PlayerShip.Pilot);
                else
                    _weaponNotPressed(3, gameTime, PlayerShip);

                if (KeyboardManager.FireWeapon4.IsBindPressed() || GamepadManager.FirePrimary.IsBindPressed())
                    _weaponPressed(4, gameTime, PlayerShip, (PlayerPilot) PlayerShip.Pilot);
                else
                    _weaponNotPressed(4, gameTime, PlayerShip);

                if (KeyboardManager.FireWeapon5.IsBindPressed() || GamepadManager.FirePrimary.IsBindPressed())
                    _weaponPressed(5, gameTime, PlayerShip, (PlayerPilot) PlayerShip.Pilot);
                else
                    _weaponNotPressed(5, gameTime, PlayerShip);





                if (KeyboardManager.FireMissile.IsBindTapped() || GamepadManager.FireMissile.IsBindTapped())
                {
                    playerShip.TryFireWeapon(gameTime, 0);
                    playerPilot.HoldingPosition = false;

                }

                if (KeyboardManager.SwitchMissile.IsBindTapped())//probably debug
                {
                    playerShip.MissileLauncher.SetMissileType(Debugging.MissileTypes.GetCurrentMoveNext());
                }
            

            #endregion

            }


            if(KeyboardManager.IsTapped(Keys.K))
            {
                PlayerShip.Kill();
            }


            if(KeyboardManager.IsTapped(Keys.R))
            {
                PlayerShip.Revive(10000, 10000);
            }

        }

        void _weaponPressed(int slot, IGameTimeService gameTime, Ship playerShip, PlayerPilot playerPilot)        {

            if (playerShip.GetWeapon(slot) is IChargable)
            {
                ((IChargable)(playerShip.GetWeapon(slot))).Charge(gameTime.ElapsedMilliseconds);
                playerShip.GetWeapon(slot).IsBeingHeld = true;
            }
            else
            {
                playerShip.TryFireWeapon(gameTime, slot);
            }
            playerPilot.HoldingPosition = false;            

        }

        private void _weaponNotPressed(int slot, IGameTimeService gameTime, Ship playerShip)
        {
            if (playerShip.GetWeapon(slot).IsBeingHeld && playerShip.GetWeapon(slot) is IChargable)
            {
                PlayerShip.TryFireWeapon(gameTime, slot);
                ((IChargable)playerShip.GetWeapon(slot)).ResetCharge();
            }
            playerShip.GetWeapon(slot).IsBeingHeld = false;


        }

        public void Draw(Camera2D camera)
        {
            foreach (var kvp in _shipList)
            {
                if (kvp.Value.IsAlive)
                    kvp.Value.Draw(camera);  
            }
        }

        public void Debug(SpriteBatch spriteBatch)
        {
            //textDrawingService.DrawTextToScreenLeft(spriteBatch, 0,
            //                                      "Position: " + ConvertUnits.ToDisplayUnits(PlayerShip.body.Position));
        }

        /// <summary>
        /// Clears shipList and removes all ship bodies
        /// Does not clear playerShip body
        /// </summary>
        public void RemoveAllShips()
        {
            int[] idsToRemove = new int[_shipList.Count];
            _shipList.Keys.CopyTo(idsToRemove, 0);
            foreach(var id in idsToRemove)
            {
                RemoveShip(id);
            }
            
            _shipList.Clear();
            _positionUpdateList.Clear();

        }
                
        public bool IsEnterModeOn()
        {
            if (_playerShipManager != null && PlayerShip != null)
                return PlayerShip.EnterMode;
            else
                return false;
        }

        public void Reset(bool savePlayerShip = true)
        {
            
            _shipList.Clear();

            if(savePlayerShip && PlayerShip != null)
            {
                
                _shipList.Add(PlayerShip.Id, PlayerShip);
                _positionUpdateList.Add(PlayerShip);
            }

        }

        private void _sendPositionUpdates(IGameTimeService gameTime)
        {

            _messageService.SendPositionUpdate(gameTime, CurrentAreaId, _positionUpdateList, _clientPlayerInfoManager==null?null:_clientPlayerInfoManager.PlayerID);//Kind of hacky, but currently necessary for simulator. Safer than having an empty _clientPlayerInfoManager
            _lastPositionUpdateTime = (float)gameTime.TotalMilliseconds;
            

        }

        private void AddShip(int shipID, Ship tempShip)
        {
            if (!_shipList.ContainsKey(shipID))
            {
                
                _shipList.Add(shipID, tempShip);
                if (_simulateNPCs && tempShip.IsLocalSim)
                {
                    _positionUpdateList.Add(tempShip);
                }


                //This stuff will probably be removed in the future.
                _targetManager.RegisterObject(tempShip);
                _teamManager.RegisterObject(tempShip);
            }
            else
            {
                throw new InvalidOperationException("Error: ship already exists in shipList!");
            }

        }

        /// <summary>
        /// Removes ship from shipList and Locally simulated ships and clears the body
        /// </summary>
        /// <param name="shipID"></param>
        public void RemoveShip(int shipID)
        {
           if(_shipList.ContainsKey(shipID))
           {
               _positionUpdateList.Remove(_shipList[shipID]);

               _targetManager.DeRegisterObject(_shipList[shipID]);
               _teamManager.DeRegisterObject(_shipList[shipID]);
               Debugging.DisposeStack.Push(this.ToString());
               _shipList[shipID].Body.Dispose();
               _shipList.Remove(shipID);
               _updateIgnoreList.Remove(shipID);
               
           }
           else
           {
               ConsoleManager.WriteLine("Error: shipID passed to ClientShipManager.RemoveShip was not found.", ConsoleMessageType.Error);
           }
            

        }

        /// <summary>
        /// Returns null if the ship is not found
        /// </summary>
        /// <param name="shipID"></param>
        /// <returns></returns>
        public Ship GetShip(int shipID)
        {
            if (_shipList.ContainsKey(shipID))
                return _shipList[shipID];
            else
                return null;
        }

        public IEnumerable<Ship> GetAllShips()
        {
            return _shipList.Values;
        }

        #region Ship Creation

        /// <summary>
        /// Used to create new network or NPC ships
        /// DO NOT USE FOR PLAYERSHIP
        /// ignorePositionUpdates should only be used by the Simulator
        /// </summary>
        public Ship CreateShip(World world, bool isNPC, Vector2 position, int shipID, float rotation, Vector2 velocity,
            string playerName,
            ShipStats shipStats, List<WeaponTypes> weaponTypes, HashSet<int> teams, bool ignorePositionUpdates = false)
        {
            Ship tempShip;


            _targetManager.DisableTargetSetting();

            switch (shipStats.ShipType)
            {
                case ShipTypes.Barge:
                    tempShip = new Barge(position, velocity, rotation, shipID, 0,
                        playerName, shipStats,
                        _particleManager,
                        world, _spriteBatch, GetTexture(shipStats.ShipType), teams);
                    tempShip.Pilot = new NetworkPilot(tempShip, new ShipBodyDataObject(BodyTypes.NetworkShip, shipID, tempShip));
                    break;

                case ShipTypes.Reaper:
                    tempShip = new Reaper(position, velocity, rotation, shipID, 0, playerName,
                        shipStats,
                        _particleManager,
                        world, _spriteBatch, GetTexture(shipStats.ShipType), teams);
                    tempShip.Pilot = new NetworkPilot(tempShip, GenerateNetworkShipBodyData(shipID, tempShip));
                    break;

                case ShipTypes.SuperCoolAwesome3DShip:
                    tempShip = new SuperCoolAwesome3DShip(_spriteBatch, GetModel(shipStats.ShipType), position, velocity, rotation, shipID, 0,
                        playerName, shipStats,
                        _particleManager,
                        world, teams);
                    tempShip.Pilot = new NetworkPilot(tempShip, new ShipBodyDataObject(BodyTypes.NetworkShip, shipID, tempShip));
                    break;


                case ShipTypes.BattleCruiser:
                    tempShip = new Battlecruiser(position, velocity, rotation, shipID, 0,
                        playerName, shipStats,
                        _particleManager,
                        world, _spriteBatch, GetTexture(shipStats.ShipType), teams);
                    tempShip.Pilot = new NetworkPilot(tempShip, GenerateNetworkShipBodyData(shipID, tempShip));
                    break;

                case ShipTypes.Penguin:
                    tempShip = new Penguin(position, velocity, rotation, shipID, 0,
                        playerName, shipStats,
                        _particleManager,
                        world, _spriteBatch, GetTexture(shipStats.ShipType), teams);
                    tempShip.Pilot = new NetworkPilot(tempShip, GenerateNetworkShipBodyData(shipID, tempShip));
                    break;

                case ShipTypes.Dread:
                    tempShip = new Dread(position, velocity, rotation, shipID, 0,
                        playerName, shipStats,
                        _particleManager,
                        world, _spriteBatch, GetTexture(shipStats.ShipType), teams);
                    tempShip.Pilot = new NetworkPilot(tempShip, GenerateNetworkShipBodyData(shipID, tempShip));

                    break;

                default:
                    ConsoleManager.WriteLine("Ship type not implemented in ClientShipManager.CreateShip",
                        ConsoleMessageType.Error);
                    return null;
            }

            if (!_simulateNPCs || tempShip.Pilot.PilotType == PilotType.NetworkPlayer)
            {
                tempShip.BodyBehaviors.Add(new LerpBodyBehavior(_spriteBatch, _textureManager, tempShip, 100));
            }

            tempShip.IsLocalSim = (_simulateNPCs && isNPC);
            AddShip(shipID, tempShip);

            byte numSet = 0;
            foreach (var t in weaponTypes)
            {
                tempShip.SetWeapon(_createWeapon(tempShip, _projectileManager, t, numSet), numSet);
                numSet++;
            }


            tempShip.Pilot = isNPC
                ? new NPCPilot(tempShip, GenerateNetworkShipBodyData(shipID, tempShip))
                : new NPCPilot(tempShip, new ShipBodyDataObject(BodyTypes.NetworkShip, shipID, tempShip));

            if (ignorePositionUpdates)
            {
                _updateIgnoreList.Add(tempShip.Id);
            }

            _targetManager.EnableTargetSetting();


            return tempShip;
        }


        protected Texture2D GetTexture(ShipTypes shipType)
        {
            if (_textureManager == null)
            {
                return null;
            }

            switch (shipType)
            {
                case ShipTypes.Barge:
                    return _textureManager.ZYVariantBarge;

                case ShipTypes.Reaper:
                    return _textureManager.Reaper;

                case ShipTypes.BattleCruiser:
                    return _textureManager.Battlecruiser;

                case ShipTypes.Penguin:
                    return _textureManager.Penguin;

                case ShipTypes.Dread:
                    return _textureManager.Dread;
                    
                default:
                    ConsoleManager.WriteLine("Texture not implemented for ShipType " + shipType, ConsoleMessageType.Error);
                    return null;
            }
        }

        protected Model GetModel(ShipTypes shipType)
        {
            if (_textureManager == null)
            {
                return null;
            }

            switch (shipType)
            {
                case ShipTypes.SuperCoolAwesome3DShip:
                    return _textureManager.SuperCoolAwesome3DShipModel;

                default:
                    ConsoleManager.WriteLine("Model not implemented for ShipType " + shipType, ConsoleMessageType.Error);
                    return null;
            }
        }


        private ShipBodyDataObject GenerateNetworkShipBodyData(int shipID, Ship tempShip)
        {
            return new ShipBodyDataObject(BodyTypes.NetworkShip, shipID, tempShip);
        }

        private CustomShipStats ReadCustomShipStatsFromMessage(NetIncomingMessage msg)
        {
            var ship = new CustomShipStats();

            try
            {
                ship.Name = msg.ReadString();
                ship.Shields = msg.ReadInt32();
                ship.Hull = msg.ReadInt32();
                ship.Energy = msg.ReadInt32();
                ship.Cargo = msg.ReadInt32();
                ship.TopSpeed = msg.ReadInt32();
                ship.Acceleration = msg.ReadInt32();
                ship.Graphic = (ShipTextures)msg.ReadByte();
                ship.ThrustGraphic = msg.ReadString();
                ship.Class = msg.ReadString();
                ship.TurnRate = msg.ReadFloat();
                ship.RegenRate = msg.ReadFloat();
            }
            catch
            {
                throw new Exception("Failed to read custom ship stats from message");
            }

            return ship;
        }
      
        //This is only here because of a weird Farseer bug with a null fixturelist...
        public Ship RecreatePlayerShip(World world)
        {
            Ship oldPlayerShip = PlayerShip;
            int oldShields = (int) PlayerShip.Shields.CurrentShields;
            int oldHealth = PlayerShip.CurrentHealth;
            int oldEnergy = PlayerShip.GetCurrentEnergy();
            bool isAlive = oldPlayerShip.IsAlive;
            var oldCargo = PlayerShip.Cargo;

            if (_shipList.ContainsKey(oldPlayerShip.Id))
            {
                _shipList.Remove(oldPlayerShip.Id);
                _positionUpdateList.Remove(oldPlayerShip);

            }

        

        var weaponsList = oldPlayerShip.GetWeaponTypes();

            CreatePlayerShip(world, oldPlayerShip.Position, oldPlayerShip.Id, oldPlayerShip.Rotation, oldPlayerShip.LinearVelocity, oldPlayerShip.playerName, oldPlayerShip.ShipStats, weaponsList, oldPlayerShip.Teams);
            PlayerShip.Shields.CurrentShields = oldShields;
            PlayerShip.CurrentHealth = oldHealth;
            PlayerShip.SetCurrentEnergy(oldEnergy);
            PlayerShip.IsAlive = isAlive;
            PlayerShip.Cargo = oldCargo;

            return PlayerShip;
        
        }

        public virtual Ship CreatePlayerShip(World world, Vector2 position, int shipID, float rotation,
            Vector2 velocity, string playerName, ShipStats shipStats, List<WeaponTypes> weaponTypes, HashSet<int> teams)
        {
            if (PlayerShip != null)
            {
                _teamManager.DeRegisterObject(PlayerShip);
                _targetManager.DeRegisterObject(PlayerShip);
                if (PlayerShip.IsBodyValid)
                {
                    Debugging.DisposeStack.Push(this.ToString());
                    PlayerShip.Body.Dispose();                    
                }
            }
            
            if (_shipList.ContainsKey(shipID))
            {
                _positionUpdateList.Remove(_shipList[shipID]);
                
                _shipList.Remove(shipID);                
            }
                        
            Ship tempShip;


            switch (shipStats.ShipType)
            {
                case ShipTypes.Barge:
                    tempShip = new Barge(position, velocity, rotation, shipID, (int)_clientPlayerInfoManager.PlayerID, playerName,
                        shipStats, _particleManager,
                        world, _spriteBatch, GetTexture(shipStats.ShipType), teams);
                    break;

                case ShipTypes.SuperCoolAwesome3DShip:
                    tempShip = new SuperCoolAwesome3DShip(_spriteBatch, GetModel(shipStats.ShipType), position, velocity, rotation, shipID, (int)_clientPlayerInfoManager.PlayerID, playerName,
                        shipStats, _particleManager,
                        world, teams);
                    break;

                case ShipTypes.Reaper:
                    tempShip = new Reaper(position, velocity, rotation, shipID, (int)_clientPlayerInfoManager.PlayerID, playerName,
                        shipStats, _particleManager,
                        world, _spriteBatch, GetTexture(shipStats.ShipType), teams);
                    break;

                case ShipTypes.BattleCruiser:
                    tempShip = new Battlecruiser(position, velocity, rotation, shipID, (int)_clientPlayerInfoManager.PlayerID, playerName,
                        shipStats, _particleManager,
                        world, _spriteBatch, GetTexture(shipStats.ShipType), teams);
                    break;

                case ShipTypes.Penguin:
                    tempShip = new Penguin(position, velocity, rotation, shipID, (int)_clientPlayerInfoManager.PlayerID, playerName,
                        shipStats, _particleManager,
                        world, _spriteBatch, GetTexture(shipStats.ShipType), teams);
                    break;

                case ShipTypes.Dread:
                    tempShip = new Dread(position, velocity, rotation, shipID, (int)_clientPlayerInfoManager.PlayerID, playerName,
                        shipStats, _particleManager,
                        world, _spriteBatch, GetTexture(shipStats.ShipType), teams);
                    break;

                default:
                    Console.WriteLine("Ship type not implemented in ClientShipManager.CreatePlayerShip");
                    return null;
            }

            byte slot = 0;
            foreach (var w in weaponTypes)
            {
                tempShip.SetWeapon(_createWeapon(tempShip, _projectileManager, w, slot), slot);
                slot++;
            }

            SetPilot(tempShip, Debugging.IsBot);

            _shipList.Add(tempShip.Id, tempShip);
            _positionUpdateList.Add(tempShip);

            _playerShipManager.PlayerShip = tempShip;
            _teamManager.RegisterObject(tempShip);

            _updateIgnoreList.Add(PlayerShip.Id);


            tempShip.CanLandWarp = true;
            return tempShip;
        }

        protected void SetPilot(Ship tempShip, bool IsBot)
        {
            if (!IsBot)
            {
                _playerShipManager.PlayerPilot = new PlayerPilot(tempShip);
                tempShip.Pilot = PlayerPilot;
                tempShip.SetUserData(new ShipBodyDataObject(BodyTypes.PlayerShip, tempShip.Id, tempShip));
            }
            else
            {
                var ud = new ShipBodyDataObject(BodyTypes.PlayerShip, tempShip.Id, tempShip);
                _playerShipManager.PlayerPilot = new NPCPilot(tempShip, ud);
                tempShip.IsLocalSim = true;
                tempShip.Pilot = PlayerPilot;
                tempShip.SetUserData(ud);

            }
        }

        protected Weapon _createWeapon(Ship s, ProjectileManager projectileManager, WeaponTypes type, byte slot)
        {
            Weapon retWeap = null;

            switch (type)
            {
                case WeaponTypes.Laser:
                    retWeap = new Laser(projectileManager, s, slot);
                    break;

                case WeaponTypes.NaniteLauncher:
                    retWeap = new NaniteLauncher(projectileManager, s, slot);
                    break;

                case WeaponTypes.None:
                    retWeap = new NullWeapon(s);
                    break;

                case WeaponTypes.BC_Laser:
                    retWeap = new BC_Laser(projectileManager, s, slot);
                    break;

                case WeaponTypes.LaserWave:
                    retWeap = new LaserWave(projectileManager, s, slot);
                    break;

                case WeaponTypes.PlasmaCannon:
                    retWeap = new PlasmaCannon(projectileManager, s, slot);
                    break;

                case WeaponTypes.HurrDurr:
                    retWeap = new HurrDurr(projectileManager, s, slot);
                    break;

                case WeaponTypes.AltLaser:
                    retWeap = new AltLaser(projectileManager, s, slot);
                    break;

                case WeaponTypes.MissileLauncher:
                    retWeap = new MissileLauncher(projectileManager, ProjectileTypes.AmbassadorMissile,  s, slot);
                    break;

                case WeaponTypes.GravBomber:
                    retWeap = new GravBomber(projectileManager, s, slot);
                    break;

                default:
                    ConsoleManager.WriteLine("Error: Invalid weapon type passed to ClientShipManager", ConsoleMessageType.Error);
                    retWeap = new NullWeapon(s);
                    break;
            }

            return retWeap;
            
        
        }
        
        #endregion
              
        public string GetPlayerEnergy(string ignored)
        {
            return PlayerShip.GetCurrentEnergy().ToString();
        }
                         

    }
}