using System;
using System.Collections.Generic;
using Freecon.Client.Interfaces;
using Microsoft.Xna.Framework;
using Core.Interfaces;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework.Graphics;
using Freecon.Client.GUI;
using Freecon.Client.Extensions;
using Freecon.Client.Managers.Networking;
using Freecon.Client.Objects;
using Freecon.Client.Objects.Structures;
using Freecon.Core.Networking.Models;
using Freecon.Client.Core.Services;
using Freecon.Client.Core.Interfaces;
using Freecon.Client.Mathematics;
using Freecon.Core.Networking.Models.Messages;

namespace Freecon.Client.Managers
{
    //TODO: Rewrite this mess
    public class SelectionManager : ISynchronousUpdate, IDraw
    {
        Dictionary<int, ISelectable> _selectedObjects = new Dictionary<int, ISelectable>();
        List<ISelectable> _localSelectedObjects = new List<ISelectable>();
        List<ISelectable> _nonLocalSelectedObjects = new List<ISelectable>();

        Dictionary<int, ICommandable> _commandableObjects = new Dictionary<int, ICommandable>();

        TextDrawingService _textDrawingService;
        SpriteBatch _spriteBatch;
        ClientShipManager _clientShipManager;
        MessageService_ToServer _messageManager;
        PhysicsManager _physicsManager;
        PlayerShipManager _playerShipManager;
        TargetingService _targetingManager;
        UIConversionService _uiConversionService;

        #region Command States
        //May be worth replacing with an enum, it's unlikely that we'll allow multiple commands per click
        bool _moveTo = false;
        bool _attack = false;
        bool _patrol = false;
        
        #endregion

        double _leftMouseDownTime;//Time mouse was clicked, to prevent instantaneous selection
        double _rightMouseDownTime;
        Vector2 _mouseDownPos;
        double _shortClickTime = 300;//ms, Minimum amount of time before button is considered "held"

        public SelectionManager(
            TextDrawingService textDrawingService,
            SpriteBatch spriteBatch,
            ClientShipManager clientShipManager,
            MessageService_ToServer messageManager,
            PhysicsManager physicsManager,
            PlayerShipManager playerShipManager,
            TargetingService targetingService,
            UIConversionService uiConversionService)
        {
            _textDrawingService = textDrawingService;
            _spriteBatch = spriteBatch;
            _clientShipManager = clientShipManager;
            _messageManager = messageManager;
            _physicsManager = physicsManager;
            _playerShipManager = playerShipManager;
            _targetingManager = targetingService;
            _uiConversionService = uiConversionService;
        }

        public void Draw(Camera2D camera)
        {
            if (_attack)
            {
                Vector2 drawPos = _uiConversionService.MousePosToSimUnits();
                drawPos *= 100;

                Vector2 mpos = MouseManager.CurrentPosition;

                //Shit don't work, yo
                //_spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, null, null, null, null, LegacyStatics.CurrentStateManager.Camera.GetTransformation(_spriteBatch.GraphicsDevice));
                _spriteBatch.Draw(TextureManager.TargetCursor, new Vector2(0, 0), null, Color.Red, 0f, new Vector2(0, 0), 100, SpriteEffects.None, 0);
                _spriteBatch.End();
            }
        }


        /// <summary>
        /// Gives commands to locally simulated NPCs, sends command to server for non-local simulated IDs
        /// </summary>
        /// <param name="command"></param>
        /// <param name="data"></param>
        void _giveCommands(SelectorCommands command, object data)
        {

            #region Local Selections
            switch (command)
            {
                case SelectorCommands.AttackTarget:
                    foreach (var s in _localSelectedObjects)
                        if (s is ICommandable)
                            ((ICommandable)s).AttackTarget(_targetingManager.GetTargetableObject((int)data));
                    break;

                case SelectorCommands.AttackToPosition:
                    foreach (var s in _localSelectedObjects)
                        if (s is ICommandable)
                            ((ICommandable)s).AttackToPosition((Vector2)data);
                    break;
                case SelectorCommands.GoToPosition:
                    foreach (var s in _localSelectedObjects)
                        if (s is ICommandable)
                            ((ICommandable)s).GoToPosition((Vector2)data);
                    break;

                case SelectorCommands.Stop:
                    foreach (var s in _localSelectedObjects)
                        if (s is ICommandable)
                            ((ICommandable)s).Stop();
                    break;

                case SelectorCommands.HoldPosition:
                    foreach (var s in _localSelectedObjects)
                        if (s is ICommandable)
                            ((ICommandable)s).HoldPosition();
                    break;

            }
            #endregion

            #region Nonlocal Selections
            List<int> nonLocalIDs = new List<int>(_nonLocalSelectedObjects.Count);
            foreach (ISelectable s in _nonLocalSelectedObjects)
                nonLocalIDs.Add(s.Id);

            //TODO: FIX THIS FIX THIS FIX THIS
            //TODO: FIX THIS FIX THIS FIX THIS
            //TODO: FIX THIS FIX THIS FIX THIS
            //_messageManager.SendSelectedCommand(command, nonLocalIDs, data);

            #endregion

        }


        /// <summary>
        /// Relays commands from server to locally simulated IDs.
        /// </summary>
        /// <param name="selectedIDs"></param>
        /// <param name="command"></param>
        /// <param name="data"></param>
        public void RelayNetworkCommand(MessageSelectorCommand commandMessage)
        {

            
            switch(commandMessage.CommandType)
            {
                case SelectorCommands.AttackTarget:
                    {
                        AttackTargetData d = commandMessage.CommandData as AttackTargetData;
                        int targetID = (d.TargetID);
                        foreach (int id in commandMessage.SelectedIDs)
                            if (_selectedObjects.ContainsKey(id) && _selectedObjects[id] is ICommandable && _selectedObjects[id].IsLocalSim)
                                ((ICommandable)_selectedObjects[id]).AttackTarget(_targetingManager.GetTargetableObject(targetID));
                        break;
                    }
                case SelectorCommands.AttackToPosition:
                    {
                        AttackToPositionData d = commandMessage.CommandData as AttackToPositionData;
                        foreach (int id in commandMessage.SelectedIDs)
                            if (_selectedObjects.ContainsKey(id) && _selectedObjects[id] is ICommandable && _selectedObjects[id].IsLocalSim)
                                ((ICommandable)_selectedObjects[id]).AttackToPosition(new Vector2(d.XPos, d.YPos));
                        break;
                    }
                case SelectorCommands.GoToPosition:
                    {
                        GoToPositionData d = commandMessage.CommandData as GoToPositionData;
                        foreach (int id in commandMessage.SelectedIDs)
                            if (_selectedObjects.ContainsKey(id) && _selectedObjects[id] is ICommandable && _selectedObjects[id].IsLocalSim)
                                ((ICommandable)_selectedObjects[id]).GoToPosition(new Vector2(d.XPos, d.YPos));
                        break;
                    }
                case SelectorCommands.Stop:
                    {
                        foreach (int id in commandMessage.SelectedIDs)
                            if (_selectedObjects.ContainsKey(id) && _selectedObjects[id] is ICommandable && _selectedObjects[id].IsLocalSim)
                                ((ICommandable)_selectedObjects[id]).Stop();
                        break;
                    }
                case SelectorCommands.HoldPosition:
                    {
                        foreach (int id in commandMessage.SelectedIDs)
                            if (_selectedObjects.ContainsKey(id) && _selectedObjects[id] is ICommandable && _selectedObjects[id].IsLocalSim)
                                ((ICommandable)_selectedObjects[id]).HoldPosition();
                        break;
                    }


            }



        }
       
        #region Click Handling

        /// <summary>
        /// Returns true if the time between last press and current time is greater than _shortClickTime
        /// </summary>
        /// <returns></returns>
        bool isLongClick(IGameTimeService gameTime, double downtime)
        {
            return gameTime.TotalMilliseconds - downtime > _shortClickTime;

        }

        /// <summary>
        /// Time between mouse down and mouse up is less than _shortClickTime
        /// </summary>
        void HandleShortLeftClick()
        {


            if (_selectedObjects.Count == 0)
            {
                _attack = false;
                return;
            }

            if (!_moveTo && !_attack && !_patrol)
            {
                ClearSelectedObjects();
            }
            else if (_attack)
            {
                _attack = false;
                ITargetable t = null;
                //Test to see if the mouse is over a targetable object
                Fixture f = _physicsManager.World.TestPoint(_uiConversionService.MousePosToSimUnits());

                if (f != null)
                {
                    //Need to store references to the actual objects in userdata to make this much prettier and simpler
                    ShipBodyDataObject s = f.Body.UserData as ShipBodyDataObject;
                    if (s != null && s.Ship is ITargetable)
                    {
                        t = s.Ship;

                    }

                    StructureBodyDataObject b = f.Body.UserData as StructureBodyDataObject;
                    if (b != null && b.Structure is ITargetable)
                    {
                        t = b.Structure;

                    }
                }

                if (t != null)
                    foreach (var s in _selectedObjects)
                    {
                        var commandable = s.Value as ICommandable;

                        if (commandable != null)
                        {
                            commandable.AttackTarget(t);
                        }

                    }
                else
                    foreach (var s in _selectedObjects)
                    {
                        var commandable = s.Value as ICommandable;

                        if (commandable != null)
                        {
                            commandable.AttackToPosition(_uiConversionService.MousePosToSimUnits());
                        }
                    }

            }
                        


        }

        /// <summary>
        /// Time between mouse down and mouse up is greater than _shortClickTime
        /// </summary>
        void HandleLongLeftClick()
        {

            //Was selecting, mouse is released, commence actual selection
            //Need to cache this body
            Vector2 mousePos = _uiConversionService.MousePosToSimUnits();
            _mouseDownPos = _uiConversionService.MousePosToSimUnits(_mouseDownPos);

            float width = Math.Abs(_mouseDownPos.X - mousePos.X);
            float height = Math.Abs(_mouseDownPos.Y - mousePos.Y);

            if (width <= 0 || height <= 0)//In case player long clicks without moving the mouse
                return;
            Debugging.AddStack.Push(this.ToString());
            Body selector = BodyFactory.CreateRectangle(_physicsManager.World, width, height, 0);
            selector.OnCollision += selector_OnCollision;

            //WARNING: Need to convert this to an AABB query
            selector.Position = new Vector2((_mouseDownPos.X + mousePos.X) / 2, (_mouseDownPos.Y + mousePos.Y) / 2);
            _physicsManager.World.ProcessChanges();//Forces collisions
            _physicsManager.World.Step(.0001f);
            Debugging.DisposeStack.Push(this.ToString());
            selector.Dispose();
            //w.ProcessChanges();

        }
        
        void HandleShortRightClick()
        {
            if (_selectedObjects.Count == 0)
                return;

            ITargetable t = null;
            //Test to see if the mouse is over a targetable object
            Fixture f = _physicsManager.World.TestPoint(_uiConversionService.MousePosToSimUnits());
            if (f != null)
            {
                //Need to store references to the actual objects in userdata to make this much prettier and simpler
                ShipBodyDataObject s = f.Body.UserData as ShipBodyDataObject;
                if (s != null && s.Ship is ITargetable)
                {
                    t = (ITargetable)s.Ship;

                }

                StructureBodyDataObject b = f.Body.UserData as StructureBodyDataObject;
                if (b != null && b.Structure is ITargetable)
                {
                    t = (ITargetable)b.Structure;

                }
            }

            if (t != null)
            {
                foreach (var s in _selectedObjects)
                {
                    ICommandable i = s.Value as ICommandable;
                    if (i != null)
                        i.AttackTarget(t);

                }
                return;
            }

            foreach (var s in _selectedObjects)
            {
                if (s.Value is Ship)
                {
                    Ship ship = (Ship) s.Value;
                    ship.Pilot.GoToPosition(_uiConversionService.MousePosToSimUnits());

                }
            }
        }

        #endregion 

        public void Clear()
        {
            _selectedObjects.Clear();
            _localSelectedObjects.Clear();
            _nonLocalSelectedObjects.Clear();
        }

        void ClearSelectedObjects()
        {
            foreach (var s in _selectedObjects)
                s.Value.IsSelected = false;

            _selectedObjects.Clear();
            _localSelectedObjects.Clear();
            _nonLocalSelectedObjects.Clear();


        }

        bool selector_OnCollision(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {


            if(fixtureB.Body.UserData is ShipBodyDataObject)
            {
                var userData = (ShipBodyDataObject)fixtureB.Body.UserData;
                Ship s = (Ship)userData.Ship;

                if (s != _clientShipManager.PlayerShip && s.OnSameTeam(_clientShipManager.PlayerShip) && !_selectedObjects.ContainsKey(s.Id))
                {
                    _selectedObjects.Add(userData.ID, (Ship)userData.Ship);
                    if (s.IsLocalSim)
                        _localSelectedObjects.Add(s);
                    else
                        _nonLocalSelectedObjects.Add(s);
                    s.IsSelected = true;
                }
            }
            else if (fixtureB.Body.UserData is StructureBodyDataObject)//Selector isn't colliding with turrets, need to see why
            {

                var userData = (StructureBodyDataObject)fixtureB.Body.UserData;
                ISelectable t = userData.Structure as ISelectable;
                if (t == null)
                    return false;

                Turret turret = t as Turret;
                

                if (turret != null && turret.OnSameTeam(_clientShipManager.PlayerShip) && !_selectedObjects.ContainsKey(turret.Id))
                {
                    _selectedObjects.Add(turret.Id, turret);
                    if (t.IsLocalSim)
                        _localSelectedObjects.Add(t);
                    else
                        _nonLocalSelectedObjects.Add(t);
                    turret.IsSelected = true;
                }
            }
            return false;

        }

        public void Update(IGameTimeService gameTime)
        {
            UpdateMouseSelection(gameTime);
            UpdateKeyboardCommands();
        }

        private void UpdateKeyboardCommands()
        {

            if (KeyboardManager.OrderAttack.IsBindTapped())
            {
                _attack = !_attack;
                //Can't get the targeting recticle to draw
                //if (_attack)
                //    MainManager.HideCursor();
                //else
                //    MainManager.ShowCursor();
            }
            else if (KeyboardManager.OrderHoldPosition.IsBindTapped())
            {

                foreach (var s in _selectedObjects)
                {
                    if (s.Value is Ship)
                    {
                        Ship ship = (Ship)s.Value;
                        ship.Pilot.HoldPosition();

                    }

                }

            }
            else if (KeyboardManager.OrderStop.IsBindTapped())
            {
                foreach (var s in _selectedObjects)
                {
                    if (s.Value is Ship)
                    {
                        Ship ship = (Ship)s.Value;
                        ship.Pilot.Stop();

                    }

                }
            }
        }

        private void UpdateMouseSelection(IGameTimeService gameTime)
        {
            if (MouseManager.LeftButtonPressed)
            {
                _mouseDownPos = MouseManager.CurrentPosition;
                _leftMouseDownTime = gameTime.TotalMilliseconds;

            }
            else if (MouseManager.LeftButtonReleased && !isLongClick(gameTime, _leftMouseDownTime))
            {
                HandleShortLeftClick();

            }
            else if (MouseManager.LeftButtonReleased && isLongClick(gameTime, _leftMouseDownTime))
            {
                HandleLongLeftClick();

            }

            if (MouseManager.RightButtonPressed)
            {
                _rightMouseDownTime = gameTime.TotalMilliseconds;

            }
            else if (MouseManager.RightButtonReleased && !isLongClick(gameTime, _rightMouseDownTime))
            {
                HandleShortRightClick();
            }
        }
    }
        
}
