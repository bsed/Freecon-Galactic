using Microsoft.Xna.Framework;
namespace Freecon.Client.Interfaces
{
    /// <summary>
    /// Interface for anything which can be targeted (ships, turrets, etc)
    /// </summary>
    public interface ITargetable : ITeamable
    {


        
        Vector2 Position { get; }

        Vector2 LinearVelocity { get; }

        TargetTypes TargetType { get; }

        bool IsBodyValid { get; }
        

        
    }
    
    public enum TargetTypes
    {
            Static,
            Moving
    }
}
