using Freecon.Client.Objects;
using Core.Interfaces;
using Freecon.Core.Networking.Models.Objects;
using Freecon.Core.Utils;
using Microsoft.Xna.Framework;

namespace Freecon.Client.Core.Extensions
{
    public static class PositionUpdateExtensions
    {
        public static PositionUpdateData GetPositionUpdate(this Ship s, IGameTimeService gameTime)
        {
            var boosting = s.ThrustStatusForServer || s.Thrusting;

            return new PositionUpdateData(
                PositionUpdateTargetType.Ship,
                s.Id,
                s.Position.X,
                s.Position.Y,
                s.Rotation,
                s.LinearVelocity.X,
                s.LinearVelocity.Y,
                s.AngularVelocity,
                s.Shields.CurrentShields,
                s.CurrentHealth,
                boosting,
                gameTime.TotalMilliseconds
            );
        }

        public static Vector2 GetPositionFromUpdate(this PositionUpdateData positionUpdate)
        {
            return new Vector2(positionUpdate.XPos, positionUpdate.YPos);
        }
    }
}
