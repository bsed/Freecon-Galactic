using System.Collections.Generic;
using Freecon.Client.Interfaces;
using Core.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Freecon.Client.Objects.Projectiles
{
    public interface IMissile: IBodyProjectile, ISimulatable, ITargeter
    {
        void CorrectVelocity(Vector2 acceleration);
       ITargetable CurrentTarget { get; set; }
        float CurrentTurnRate { get; }
        void Draw(SpriteBatch spriteBatch);
        Vector2 DrawPos { get; }
        bool IsAlliedWithPlanetOwner { get; set; }
        bool IsLocalSim { get; set; }
        Vector2 LinearVelocity { get; set; }
        Vector2 Position { get; set; }
        Dictionary<int, ITargetable> PotentialTargets { get; set; }
        void Simulate(IGameTimeService gameTime);
        HashSet<int> Teams { get; set; }
        void Terminate();
        void ThrustForward();
        void Update(IGameTimeService gameTime);

        ICanFire FiringObj { get; }

        float Health { get; set; }
    }
}
