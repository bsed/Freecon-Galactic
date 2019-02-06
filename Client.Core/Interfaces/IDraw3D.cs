using Freecon.Client.Core.Objects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Freecon.Client.Core.Interfaces
{
    public interface IDraw3D
    {
        DrawData3D DrawData { get; }

        Vector2 Position { get; }

        float Rotation { get; }

        Model DrawModel { get; }
        
    }
}
