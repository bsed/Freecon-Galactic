using Microsoft.Xna.Framework;
namespace Freecon.Client.Interfaces
{
    interface ICommandable
    {
        void HoldPosition();

        void GoToPosition(Vector2 destination);

        void AttackToPosition(Vector2 destination);

        void Stop();

        void AttackTarget(ITargetable target);
        
    }
}
