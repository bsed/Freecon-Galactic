using Microsoft.Xna.Framework.Graphics;
using Freecon.Client.Objects;

namespace Freecon.Client.Managers.Invasion
{
    public class TileManager
    {
        public static Tile[][] Tiles;

        /// <summary>
        /// Creates instance of Tile Manager. Holds data used for rendering.
        /// </summary>
        /// <param name="xDimension">Width of map</param>
        /// <param name="yDimension">Height of map</param>
        public TileManager(int xDimension, int yDimension)
        {
            Tiles = new Tile[xDimension][];
            for (int i = 0; i < xDimension; i++)
            {
                Tiles[i] = new Tile[yDimension];
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
        }
    }

    public enum TileTypes
    {
        Wall,
        Ground,
        Cracked,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        LeftWall,
        RightWall,
        BottomWall,
        TopWall,
        TopBottom,
        LeftRight,
        LeftEnd,
        TopEnd,
        RightEnd,
        BottomEnd,
        TurretGround
    }
}