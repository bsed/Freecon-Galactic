using Microsoft.Xna.Framework;
namespace Freecon.Client.Objects
{
    public class QueuedMissile
    {
        public Vector2 Position;
        public float rotation;
        public int type;

        public QueuedMissile(Vector2 pos, float rotation, int type)
        {
            Position = pos;
            this.rotation = rotation;
            this.type = type;
        }
    }
}