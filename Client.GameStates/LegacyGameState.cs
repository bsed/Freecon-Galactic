using Core.Models.Enums;
using FarseerPhysics;
using Freecon.Models.TypeEnums;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Freecon.Client.Interfaces;
using Freecon.Client.Mathematics;
using Freecon.Client.Objects.Structures;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Freecon.Client.Managers.States
{
    /// <summary>
    /// Base GameState class, all GameStateManagers should inherit from this
    /// </summary>
    public class LegacyGameState
    {
        protected ParticleManager _particleManager;
        protected PhysicsManager _physicsManager;
        protected TargetingService _targetManager;
        protected TeamManager _teamManager;

        protected double _lastTimeStamp = 0;
               
        public Camera2D Camera { get; set; }

        //This is a tad sloppy since certain GameStates like port don't have structures, may be a good idea to make
        //an additional PlayableGameState to inherit from GameState and be inherited by states like PlanetStateManager
        protected IList<Structure> _structures;

        public virtual float Zoom
        {
            get { return Camera.Zoom; }
            set { Camera.Zoom = value; }
        }

        public LegacyGameState(
            ParticleManager pm,
            PhysicsManager physicsManager,
            TargetingService targetManager,
            TeamManager teamManager)
        {
            _particleManager = pm;
            _physicsManager = physicsManager;
            _targetManager = targetManager;
            _teamManager = teamManager;

            _structures = new List<Structure>();
        }

        public virtual void CreateStructure(float xPos, float yPos, StructureTypes buildingType, float health,
                                            float constructionPoints, int ID, HashSet<int> teams)
        {

        }

        public virtual void removeStructure(int ID)
        {

        }
     
        public void StructFireDenial(NetIncomingMessage msg)
        {
            int structureID = msg.ReadInt32();

            var structure = GetStructureByID(structureID);

            if (structure != null) // Denial may be received after kill message
            {
                structure.WaitingForFireResponse = false;
            }
        }
        

        public virtual void KillStructure(int structureID)
        {
            var structure = GetStructureByID(structureID);

            if (structure != null)
            {

                Vector2 effectPos = new Vector2(structure.xPos, structure.yPos);

                _particleManager.TriggerEffect(ConvertUnits.ToDisplayUnits(effectPos), ParticleEffectType.ExplosionEffect, 5);

                if (structure is ITargetable)
                {
                    Debugging.DisposeStack.Push(this.ToString());
                    structure.Kill();
                    _targetManager.DeRegisterObject(structure);

                    if (structure is Turret) // Need to get rid of this
                        ((Turret)structure).IsBodyValid = false;

                }

                _structures.Remove(structure);
            }
        }

        protected virtual void SetPotentialTargets()
        {

        }

        /// <summary>
        /// Returns a reference to the structure with the passed ID if it exists, null otherwise
        /// </summary>
        /// <param name="structureID"></param>
        /// <returns></returns>
        public Structure GetStructureByID(int structureID)
        {
            return _structures.FirstOrDefault(p => p.Id == structureID);
        }

        public IEnumerable<Turret> FindAllTurrets()
        {
            return this._structures.Where(p => p.StructureType == StructureTypes.LaserTurret).Select(p => p as Turret).Where(p => p != null);
        }
    }

    public class StructureRemovedEventArgs : EventArgs
    {
        public Structure RemovedStructure { get; private set; }

        public StructureRemovedEventArgs(Structure structure)
        {
            RemovedStructure = structure;
        }
    }
}
