using System;
using Freecon.Client.Objects.Pilots;
using Freecon.Client.Objects;
using Core.Interfaces;
using Microsoft.Xna.Framework;

namespace Freecon.Client.Core.Coroutines
{
    /// <summary>
    /// Pauses execution of a coroutine for the specified interval
    /// </summary>
    public class Wait : Routine
    {
        protected float _elapsed;

        public float Interval { get; protected set; }

        /// <summary>
        /// Waits until the next game loop iteration to continue execution
        /// </summary>
        /// <returns></returns>
        public static Wait ForNextUpdate()
        {
            return new Wait();
        }

        public static Wait Seconds(float seconds)
        {
            return new Wait(seconds * 1000f);
        }

        public Wait()
        {
            Done = false;
            Interval = 0;
        }

        public Wait(float interval)
        {
            Done = false;
            Interval = interval;
        }

        public override void Execute()
        {
            _elapsed = 0f;
        }

        public override void Update(IGameTimeService gameTime)
        {
            _elapsed += gameTime.ElapsedMilliseconds;

            if (_elapsed >= Interval)
            {
                Done = true;
            }
        }
    }

    public class TurnTowards : Routine
    {
        protected Ship Ship { get { return Pilot.Ship; } }

        public NPCPilot Pilot { get; protected set; }

        public Func<Vector2> GetTarget { get; protected set; }

        public float Tolerance { get; protected set; }

        public TurnTowards(NPCPilot pilot, float tolerance, Func<Vector2> getTarget)
        {
            Pilot = pilot;
            Tolerance = tolerance;
            GetTarget = getTarget;
        }

        public override void Execute()
        {
        }

        public override void Update(IGameTimeService gameTime)
        {
            var target = GetTarget();

            if ((Ship.Position - target).Length() < Tolerance && Ship.LinearVelocity.Length() <= .1)
            {
                // Tolerance is randomized so all ships don't move to the same location
                // Should probably move tolerance outside of this loop so that it isn't set on every iteration
                if ((Ship.Position - target).Length() < 3f && Ship.LinearVelocity.Length() > .1f)
                {
                    // Ship is within range of the destination, start slowing down
                    //StopShip(_lastGameTime);
                }
            }
            Done = true;
        }
    }
}