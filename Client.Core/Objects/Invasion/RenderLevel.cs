using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Freecon.Client.Managers;
using Freecon.Client.Managers.Invasion;
using Freecon.Client.Mathematics;
using FarseerPhysics;

namespace Freecon.Client.Objects
{
    public class RenderLevel
    {
        private Boundaries drawArea;
        private int left, right, top, bottom;

        private TextureManager _textureManager;

        /// <summary>
        /// Takes a tile type and returns a texture. Useful for condensing the code.
        /// </summary>
        private Dictionary<TileTypes, Texture2D> TypeToTexture;

        public RenderLevel(TextureManager textureManager)
        {
            _textureManager = textureManager;

            TypeToTexture = new Dictionary<TileTypes, Texture2D>
            {
                { TileTypes.Wall, _textureManager.Wall },
                { TileTypes.Ground, _textureManager.Ground },
                { TileTypes.Cracked, _textureManager.Cracked },
                { TileTypes.TopLeft, _textureManager.TopLeft },
                { TileTypes.TopRight, _textureManager.TopRight },
                { TileTypes.BottomLeft, _textureManager.BottomLeft },
                { TileTypes.BottomRight, _textureManager.BottomRight },
                { TileTypes.LeftWall, _textureManager.LeftWall },
                { TileTypes.RightWall, _textureManager.RightWall },
                { TileTypes.BottomWall, _textureManager.BottomWall },
                { TileTypes.TopWall, _textureManager.TopWall },
                { TileTypes.TopBottom, _textureManager.TopBottom },
                { TileTypes.LeftRight, _textureManager.LeftRight },
                { TileTypes.LeftEnd, _textureManager.LeftEnd },
                { TileTypes.TopEnd, _textureManager.TopEnd },
                { TileTypes.RightEnd, _textureManager.RightEnd },
                { TileTypes.BottomEnd, _textureManager.BottomEnd },
                { TileTypes.TurretGround, _textureManager.TurretBase }
            };
        }

        public virtual void Update()
        {
        }

        public virtual void Draw(SpriteBatch spriteBatch, Camera2D layoutCamera, float TileSize, PlanetLevel level)
        {
            
            drawArea = GetDrawTiles(spriteBatch, layoutCamera.Pos, TileSize, layoutCamera.Zoom, level);

            // Get variables from Vector Array
            right = (int)drawArea.Right;
            left = (int)drawArea.Left;
            top = (int)drawArea.Top;
            bottom = (int)drawArea.Bottom;

            for (int y = top; y <= bottom; y++)
                for (int x = left; x <= right; x++)
                {
                    if (x < 0 || x > level.PlanetMap.GetLength(0) - 1 ||
                            y < 0 || y > level.PlanetMap.GetLength(1) - 1) // Draw into the infinite distance.
                    {
                        DrawTile(spriteBatch, new Tile { position = new Vector2(x * TileSize, y * TileSize), type = TileTypes.Wall });
                        continue;
                    }

                    DrawTile(spriteBatch, level.PlanetMap[x, y]);
                }
        }

        private void DrawTile(SpriteBatch spriteBatch, Tile tile)
        {
            spriteBatch.Draw(this.TypeToTexture[tile.type], new Vector2((float)Math.Floor(ConvertUnits.ToDisplayUnits(tile.position.X)),
                                                                        (float)Math.Floor(ConvertUnits.ToDisplayUnits(tile.position.Y))), 
                                                            Color.White);
        }

        /// <summary>
        /// Returns the X's and Y's to draw. Min and Max.
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="cameraPosition"></param>
        /// <param name="Tile"></param>
        /// <returns></returns>
        private Boundaries GetDrawTiles(SpriteBatch spriteBatch, Vector2 cameraPosition, float TileSize, float zoom,
                                      PlanetLevel level)
        {
            TileSize = ConvertUnits.ToDisplayUnits(TileSize);

            // Tiles need to be resized depending upon zoom level. Performance optimization
            float ZoomedTile = TileSize * zoom;

            // Everything is relative to the ship
            Vector2 TileShipIsOver = new Vector2(
                    // Ceiling indicates the max. A few pixels offscreen is better than black bar!
                    (float)Math.Floor(cameraPosition.X / TileSize),
                    (float)Math.Floor(cameraPosition.Y / TileSize));

            float width, height; // # of tiles across screen
            width = (float)Math.Ceiling(spriteBatch.GraphicsDevice.Viewport.Width / ZoomedTile);
            height = (float)Math.Ceiling(spriteBatch.GraphicsDevice.Viewport.Height / ZoomedTile);

            // Define boundaries for drawing, this is hackish still.
            Boundaries boundaries = new Boundaries();
            boundaries.Left = (int)(TileShipIsOver.X - Math.Floor(width / 2f)) - 1; // Left
            boundaries.Right = (int)(TileShipIsOver.X + Math.Ceiling(width / 2f)) + 3; // Right
            boundaries.Top = (int)(TileShipIsOver.Y - Math.Floor(height / 2f)) - 1; // Top
            boundaries.Bottom = (int)(TileShipIsOver.Y + Math.Ceiling(height / 2f)) + 4; // Bottom

            return boundaries;
        }

        struct Boundaries
        {
            public float Left, Right, Top, Bottom;

            public override string ToString()
            {
                return "Left: " + Left + " Right: " + Right + " Top: " + Top + " Bottom: " + Bottom;
            }
        }
    }
}
