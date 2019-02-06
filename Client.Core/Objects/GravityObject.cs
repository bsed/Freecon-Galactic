using Core.Models.Enums;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using Freecon.Client.Managers;
using System.Collections.Generic;
using FarseerPhysics.Dynamics;
using Freecon.Core.Utils;

namespace Freecon.Client.Core.Objects
{

    public class GravityObject
    {
        public Vector2 Position;
        public float GravVal;

        float lastTriggerTime = 0;
        float drawPeriod = 10;//ms

        public GravityObject(Vector2 position, float gravVal)
        {
            Position = position;
            GravVal = gravVal;
        }

        /// <summary>
        /// Applies gravity to all objects in objectBodies list, except those with BodyData IDs specified in idsToIgnore
        /// </summary>
        /// <param name="objectBodies"></param>
        /// <param name="idsToIgnore"></param>
        public void Gravitate(IEnumerable<Body> objectBodies)
        {
            foreach (var s in objectBodies)
            {
                if (s.IsDisposed)
                    continue;

                Vector2 forceDir = (s.Position - Position);
                float dist = forceDir.Length();
                forceDir.Normalize();

                if (dist < .5f)
                    dist = .5f;

                forceDir = -forceDir;

                s.ApplyLinearImpulse(forceDir * GravVal / (dist * dist));//Inverse square gravity

            }

        }

        public void TriggerParticleEffect(ParticleManager pm)
        {
            if (TimeKeeper.MsSinceInitialization - lastTriggerTime > drawPeriod)
            {
                lastTriggerTime = TimeKeeper.MsSinceInitialization;
                pm.TriggerEffect(ConvertUnits.ToDisplayUnits(Position), ParticleEffectType.WarpHoleEffect, .2f);
            }
        }
    }
}
