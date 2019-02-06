using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Freecon.Models.TypeEnums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Freecon.Client.Managers;
using Freecon.Client.Managers.Networking;
using Freecon.Client.Objects.Structures;
using System;
using System.Collections.Generic;
using Freecon.Client.Core.Objects.Invasion;
using Freecon.Core.Networking.Models.Objects;
using Server.Managers;

namespace Freecon.Client.Core.Factories
{
    public class StructureFactoryManager
    {
        Random _random;

        MessageService_ToServer _messageManager;
        World _world;
        ProjectileManager _projectileManager;
        TargetingService _targetManager;
        TeamManager _teamManager;
        TextureManager _textureManager;
        ClientShipManager _clientShipManager;
        SpriteBatch _spriteBatch;

        /// <summary>
        /// If true, sets Structure.IsLocalSim to true. Should be true for simulator.
        /// </summary>
        private bool _simulateStructures;


        public StructureFactoryManager(
            MessageService_ToServer messageManager,
            World world,
            ProjectileManager projectileManager,
            TargetingService targetManager,
            TeamManager teamManager,
            TextureManager textureManager,
            ClientShipManager clientShipManager,
            SpriteBatch spriteBatch,
            bool simulateStructures
            )
        {
            _random = new Random();
            _messageManager = messageManager;
            _world = world;
            _projectileManager = projectileManager;
            _targetManager = targetManager;
            _teamManager = teamManager;
            _textureManager = textureManager;
            _clientShipManager = clientShipManager;
            _spriteBatch = spriteBatch;
            _simulateStructures = simulateStructures;
        }


        public Structure CreateStructure(StructureData structureData)
        {
            Structure structure = null;

            // TODO: Ensure unique IDs.
            switch ((StructureTypes)structureData.StructureType)
            {
                case (StructureTypes.LaserTurret):
                    structure = CreateTurret(new Vector2(structureData.XPos, structureData.YPos), structureData.CurrentHealth, structureData.Id, (TurretTypes)((TurretData)structureData).TurretType, ((TurretData)structureData).OwnerTeamIDs);
                    break;

                case (StructureTypes.CommandCenter):
                    structure = CreateCommandCenter(structureData.XPos, structureData.YPos, structureData.CurrentHealth, structureData.Id, structureData.OwnerTeamIDs);
                    break;

                case (StructureTypes.DefensiveMine):
                    structure = CreateDefensiveMine(structureData.XPos, structureData.YPos, 100, structureData.Id,
                        structureData.OwnerTeamIDs);
                    break;

                default:
                    // Need to move this
                    structure = CreateGenericStructure(structureData.XPos, structureData.YPos, (StructureTypes)structureData.StructureType, structureData.CurrentHealth, 0, structureData.Id, structureData.OwnerTeamIDs);
                    break;
            }

            structure.IsLocalSim = _simulateStructures;
            return structure;
        

        }



        public CommandCenter CreateCommandCenter(
            float xPos, float yPos, float health, int ID, HashSet<int> teams)
        {
            var tex = _textureManager == null ? null : _textureManager.CommandCenter;

            var structure = new CommandCenter(_spriteBatch,
                _messageManager.SendEnterColonyRequest, 
                _clientShipManager.IsEnterModeOn, _world, tex,
                new Vector2(xPos, yPos), StructureTypes.CommandCenter, health, ID, teams);

            RegisterStructure(structure);
            structure.IsLocalSim = _simulateStructures;

            return structure;
        }

        public DefensiveMine CreateDefensiveMine(
            float xPos, float yPos, float health, int ID, HashSet<int> teams)
        {
            Texture2D blinkOnTex = _textureManager == null ? null : _textureManager.MineOn;
            Texture2D blinkOffTex = _textureManager == null ? null : _textureManager.MineOff;

            var structure = new DefensiveMine(_world, _spriteBatch, ID, _messageManager, teams, new Vector2(xPos, yPos),
                0, blinkOffTex, blinkOnTex, _projectileManager);

            RegisterStructure(structure);
            structure.IsLocalSim = _simulateStructures;

            return structure;
        }

        public Structure CreateGenericStructure(
            float xPos,
            float yPos,
            StructureTypes structureType,
            float health,
            float constructionPoints,
            int ID,
            HashSet<int> teams)
        {
            var structure = new Structure(
                _spriteBatch, GetTexture(structureType), xPos, yPos, structureType, health, ID, teams);
            Debugging.AddStack.Push(this.ToString());
            structure.SetBody(BodyFactory.CreateCircle(
                _world, 1, 1, new StructureBodyDataObject(BodyTypes.Structure, structure)));

            RegisterStructure(structure);
            structure.IsLocalSim = _simulateStructures;

            return structure;
        }

        public Turret CreateTurret(
            Vector2 position,
            float health,
            int ID,
            TurretTypes turretType,
            HashSet<int> teams
            )
        {
            var turret = new Turret(_messageManager,
                _projectileManager, GetTexture(StructureTypes.LaserTurret, false), GetTexture(StructureTypes.LaserTurret, true), _clientShipManager, _spriteBatch,
                _world, position, StructureTypes.LaserTurret,
                health, ID, _random.Next(int.MaxValue), turretType, teams
            );

            turret.IsAlliedWithPlanetOwner = true;//True by default, may change later
            RegisterStructure(turret);
            turret.IsLocalSim = _simulateStructures;

            return turret;
        }
     
        /// <summary>
        /// headOrBase is a temporary hack used only for the turret
        /// </summary>
        /// <param name="structureType"></param>
        /// <param name="headOrBase"></param>
        /// <returns></returns>
        Texture2D GetTexture(StructureTypes structureType, bool headOrBase = false)
        {

            if (_textureManager == null)
                return null;

            switch (structureType)
            {
                case StructureTypes.CommandCenter:
                    return _textureManager.CommandCenter;                    
                case StructureTypes.LaserTurret:
                    if (headOrBase)
                        return _textureManager.TurretHead;
                    else
                        return _textureManager.TurretBase;
                case StructureTypes.Biodome:
                    return _textureManager.Biodome;                 
                default:
                    ConsoleManager.WriteLine("Error, texture type " + structureType + "not yet implemented in " + this, ConsoleMessageType.Error);
                    return null;
            }

        }

        private void RegisterStructure(Structure turret)
        {
            _teamManager.RegisterObject(turret);
            _targetManager.RegisterObject(turret);
        }
    }
}
