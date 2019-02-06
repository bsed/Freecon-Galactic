using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Freecon.Client.Objects;
using Freecon.Client.Managers.Invasion;

namespace Freecon.Client.Mathematics
{
    internal class ParseBitmap
    {
        private Color bottomLeftCorner = Color.Red;
        private Color bottomRightCorner = Color.Yellow;
        private Color bottomWall = new Color(0, 127, 255);
        private Color groundColor = Color.White;
        private Color landColor = Color.GhostWhite; //Colors correspond to a texture.
        private Color leftWall = Color.Cyan;
        private Color rightWall = new Color(0, 255, 0);
        private Color[] tempColor = new Color[1]; //Used when getting colors for Bitmap.
        private Color topLeftCorner = Color.Blue;
        private Color topRightCorner = new Color(255, 127, 127);
        private Color topWall = new Color(255, 127, 0);
        private Color wallColor = Color.Black; //Colors used when reading bitmap file.

        /// <summary>
        /// Builds a module from given input bitmap.
        /// </summary>
        /// <param name="bitmap">Bitmap to generate from.</param>
        /// <returns>Module with Colors and Compatibility attached.</returns>
        public Tile[,] Parse(Texture2D bitmap)
        {
            var m = new Tile[bitmap.Width,bitmap.Height];
            for (int y = 0; y < m.GetLength(1); y++)
                for (int x = 0; x < m.GetLength(0); x++)
                {
                    m[x, y] = new Tile { position = new Vector2(x, y) };
                }

            m = moduleBuilder(bitmap, m);
            return m;
        }

        /// <summary>
        /// Builds a module with Colors parsed and compatibility for all 4 sides.
        /// </summary>
        /// <param name="b">Bitmap to parse.</param>
        /// <param name="m">Module to add data to.</param>
        /// <returns>Returns built module.</returns>
        private Tile[,] moduleBuilder(Texture2D b, Tile[,] m)
        {
            m = getColors(b, m);
            return m;
        }

        /// <summary>
        /// Parses bitmap colors to determine map.
        /// </summary>
        /// <param name="b">Bitmap to get data from.</param>
        /// <param name="m">Module to attach StringMap to.</param>
        /// <returns>Returns module with StringMap attached.</returns>
        private Tile[,] getColors(Texture2D b, Tile[,] m)
        {
            // Parses bitmap colors to determine map.
            for (int x = 0; x < b.Width; x++)
            {
                for (int y = 0; y < b.Height; y++)
                {
                    #region StringMap Setting

                    b.GetData<Color>
                        (0, new Rectangle(x, y, 1, 1), tempColor, 0, 1);

                    // Unfortunately Color is not capable of being in a Switch() :(
                    if (tempColor[0] == wallColor)
                    {
                        m[x, y].type = TileTypes.Wall;
                    }
                    else if (tempColor[0] == groundColor)
                    {
                        m[x, y].type = TileTypes.Ground;
                    }
                    else if (tempColor[0] == landColor)
                    {
                        m[x, y].type = TileTypes.Cracked;
                    }
                    else if (tempColor[0] == topLeftCorner)
                    {
                        m[x, y].type = TileTypes.TopLeft;
                    }
                    else if (tempColor[0] == topRightCorner)
                    {
                        m[x, y].type = TileTypes.TopRight;
                    }
                    else if (tempColor[0] == bottomLeftCorner)
                    {
                        m[x, y].type = TileTypes.BottomLeft;
                    }
                    else if (tempColor[0] == bottomRightCorner)
                    {
                        m[x, y].type = TileTypes.BottomRight;
                    }
                    else if (tempColor[0] == leftWall)
                    {
                        m[x, y].type = TileTypes.LeftWall;
                    }
                    else if (tempColor[0] == rightWall)
                    {
                        m[x, y].type = TileTypes.RightWall;
                    }
                    else if (tempColor[0] == bottomWall)
                    {
                        m[x, y].type = TileTypes.BottomWall;
                    }
                    else if (tempColor[0] == topWall)
                    {
                        m[x, y].type = TileTypes.TopWall;
                    }

                    #endregion
                }
            }
            return m;
        }
    }
}