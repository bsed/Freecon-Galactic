using System;
using Microsoft.Xna.Framework;
using Core.Interfaces;
using Freecon.Client.Interfaces;

namespace Freecon.Client.Objects.Pilots
{
    public abstract class Pilot
    {
        public ITargetable CurrentTarget 
        {
            get { return _currentTarget; }

            set {
                _currentTarget = value;                
            }
        }

        
        ITargetable _currentTarget;

        public PilotType PilotType { get; set; }

        public Ship Ship { get; set; }

        protected Random _rand = new Random(6464);

        public bool HoldingPosition { get; set; }

        public bool IsAlive { get; set; }

        public virtual void Update(IGameTimeService gameTime)
        {
        }

        public virtual void Simulate(IGameTimeService gameTime)
        {
        }

        /// <summary>
        /// Allows the pilot to logically choose a new target
        /// </summary>
        /// <param name="firingObject"></param>
        public virtual void HitByTargetable(ITargetable firingObject)
        {
        }

        public virtual void HoldPosition()
        {
            HoldingPosition = true;
        }

        public virtual void GoToPosition(Vector2 destination)
        { }

        public virtual void AttackToPosition(Vector2 destination)
        {}

        public virtual void Stop()
        { }

        public virtual void AttackTarget(ITargetable target)
        { }
        
    }
}
