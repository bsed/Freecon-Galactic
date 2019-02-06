using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Freecon.Client.Managers;
using Freecon.Client.Objects;


namespace Freecon.Client.Mathematics.TileEditor
{
    public class SelectTile
    {
        private Vector2 modifyTransformation;
        private Vector2 renderMouseLocation;

        private Vector2 t,
                        transformation;

        public SelectTile(int Size)
        {
        }

        /// <summary>
        /// Gets tile to change value of.
        /// </summary>
        /// <param name="spriteBatch">A spritebatch</param>
        /// <param name="mouse"></param>
        /// <param name="cameraPosition"></param>
        /// <param name="TileSize"></param>
        /// <param name="zoom"></param>
        /// <returns></returns>
        public Boolean setTileFromClick(SpriteBatch spriteBatch, int TileSize,
                                        Camera2D layoutCam, ref PlanetLevel level, int leftTool, int rightTool,
                                        bool hudUp)
        {
            if (MouseManager.LeftButtonDown ||
                MouseManager.RightButtonDown)
            {
                int width = (int) (TileSize*layoutCam.Zoom), height = (int) (TileSize*layoutCam.Zoom);
                    // Calculate size of tile.

                renderMouseLocation.X = MouseManager.CurrentPosition.X // Get X tile.
                                        +
                                        (int)
                                        Math.Floor(((layoutCam.Pos.X*layoutCam.Zoom) -
                                                    spriteBatch.GraphicsDevice.Viewport.Width/2));

                renderMouseLocation.Y = MouseManager.CurrentPosition.Y // Get Y tile.
                                        +
                                        (int)
                                        Math.Floor(((layoutCam.Pos.Y*layoutCam.Zoom) -
                                                    spriteBatch.GraphicsDevice.Viewport.Height/2));

                if (renderMouseLocation.X < 0 || renderMouseLocation.Y < 0) // If out of map
                {
                    return false;
                }

                if (renderMouseLocation.X >= level.PlanetMap.GetLength(0)*TileSize // If out of map
                    || renderMouseLocation.Y >= level.PlanetMap.GetLength(1)*TileSize)
                {
                    return false;
                }

                var tileMouse = new Vector2( // Current Tile Selected, compensating for Zoom.
                    (int) (Math.Floor((renderMouseLocation.X + (TileSize/2)*layoutCam.Zoom)/(TileSize*layoutCam.Zoom))),
                    (int) (Math.Floor((renderMouseLocation.Y + (TileSize/2)*layoutCam.Zoom)/(TileSize*layoutCam.Zoom))));

                if (tileMouse.X >= level.PlanetMap.GetLength(0)) // Check if out of map
                {
                    return false;
                }

                if (tileMouse.Y >= level.PlanetMap.GetLength(1)) // Check if out of map
                {
                    return false;
                }
                if (!hudUp)
                {
                    /*
                    if (MouseManager.currentState.LeftButton == ButtonState.Pressed) // Set based on left tool
                        level.PlanetMap[(int)tileMouse.X, (int)tileMouse.Y].tileType = leftTool;
                    else if (MouseManager.RightButtonPressed) // Set based on right tool
                        level.PlanetMap[(int)tileMouse.X, (int)tileMouse.Y].tileType = rightTool;
                     * */
                }
            }
            return true;
        }

        /// <summary>
        /// Gets distance between 2 Vector2Ds.
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <returns></returns>
        private static Vector2 vectorDifference(Vector2 vector1, Vector2 vector2)
        {
            int xdiff = (int) (vector2.X - vector1.X), ydiff = (int) (vector2.Y - vector1.Y);
            return new Vector2(xdiff, ydiff);
        }


        public void MouseInput()
        {
            if (MouseManager.LeftButtonPressed)
                // Check if mouse was previously released and is now pressed.
            {
                modifyTransformation = new Vector2(MouseManager.CurrentPosition.X, MouseManager.CurrentPosition.Y);
            }

            // Detect if mouse moved
            if (MouseManager.LeftButtonDown &&
                MouseManager.OldPosition.X != MouseManager.CurrentPosition.X
                ||
                MouseManager.LeftButtonDown &&
                MouseManager.OldPosition.Y != MouseManager.CurrentPosition.Y)
            {
                t = vectorDifference(modifyTransformation,
                                     new Vector2(MouseManager.CurrentPosition.X, MouseManager.CurrentPosition.Y));
                t = t/30;
                transformation += t;
            }

            if (MouseManager.LeftButtonPressed)
                // Check if was previously clicked and is now released.
            {
                t = Vector2.Zero; // Set map Transformation to Zero when mouse is Released.
            }
        }
    }
}