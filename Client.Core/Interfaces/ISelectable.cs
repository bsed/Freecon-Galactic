using Microsoft.Xna.Framework;
namespace Freecon.Client.Interfaces
{
    /// <summary>
    /// Objects that can be selected, starcraft style
    /// </summary>
    interface ISelectable
    {
        int Id { get; }

        bool IsBodyValid { get; }

        bool IsSelected { get; set; }

        Vector2 Position { get; }

        bool IsLocalSim { get; }

    }
}
