using Microsoft.Xna.Framework.Graphics;
using Freecon.Client.Managers.Invasion;
using Microsoft.Xna.Framework;
namespace Freecon.Client.Objects
{
    public class Tile
    {
        public TileTypes type;
        public Vector2 position;
        //public int tileType = -1; // Make into enum
        //public bool isWall = false;
        //public bool isTurret = false;
        //public bool isWarp = false;
        public virtual void Draw(SpriteBatch spriteBatch)
        {
        }
    }
}