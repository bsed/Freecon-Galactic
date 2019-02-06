using System.Collections.Generic;
using Microsoft.Xna.Framework;
namespace Freecon.Client.Interfaces
{
    /// <summary>
    /// Any object which can hold and fire a weapon
    /// </summary>
    public interface ICanFire
    {
        Vector2 LinearVelocity { get; }

        Vector2 Position { get; }

        float Rotation { get; }

        bool IsBodyValid { get; }

        void ChangeEnergy(int amount);

        float BodyHeight { get; }

        int GetCurrentEnergy();

        void SetCurrentEnergy(int value);

        int Id { get; }

        HashSet<int> Teams { get; }



    }
}
