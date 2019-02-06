using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Freecon.Client.Managers;


namespace Freecon.Client.Mathematics.TileEditor
{
    public class SelectionHUD
    {
        // Selection box to draw.
        private readonly Texture2D SelectionBox;
        private readonly Texture2D SelectionLeft;
        private readonly Texture2D SelectionRight;

        // Tool specifies tile that will be placed upon click.

        // Specifies size of each cell in selection box.
        private int cellSizeX = 80, cellSizeY = 80;
        public int currentRightTool = 1;
        public int currentTool = 0;

        // Scale of viewport/screen, used for selection.
        private int gameHeight;
        private int gameWidth;

        private bool hud_Down = true;
        private bool hud_Moving;
        private Vector2 hud_Pos;
        private bool hud_Up;
        private bool isInHud; // Used to determine if mouse is inside selection screen.
        private Vector2 selectionLeft_Pos;
        private Vector2 selectionRight_Pos;

        public SelectionHUD(ContentManager Content)
        {
            SelectionBox = Content.Load<Texture2D>(@"GUI/HUD2");
            SelectionLeft = Content.Load<Texture2D>(@"GUI/SelectionLeft");
            SelectionRight = Content.Load<Texture2D>(@"GUI/SelectionRight");
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            // Gets current size of screen.
            gameWidth = spriteBatch.GraphicsDevice.Viewport.Width;
            gameHeight = spriteBatch.GraphicsDevice.Viewport.Height;

            hud_Pos.X = (spriteBatch.GraphicsDevice.Viewport.Width / 2) - (SelectionBox.Width / 2);

            if (hud_Pos.Y < (spriteBatch.GraphicsDevice.Viewport.Height - SelectionBox.Height))
            {
                hud_Pos.Y = (spriteBatch.GraphicsDevice.Viewport.Height - SelectionBox.Height);
                hud_Up = true;
            }


            // Draws selection box to screen at bottom center of screen.
            spriteBatch.Draw(SelectionBox,
                             new Rectangle((int)(hud_Pos.X), (int)(hud_Pos.Y), SelectionBox.Width, SelectionBox.Height),
                             new Rectangle(0, 0, SelectionBox.Width, SelectionBox.Height),
                             Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);

            if (hud_Up)
            {
                //Draws selection box over selected tool
                spriteBatch.Draw(SelectionRight,
                                 new Rectangle((int)(selectionRight_Pos.X), (int)(selectionRight_Pos.Y),
                                               SelectionRight.Width, SelectionRight.Height),
                                 new Rectangle(0, 0, SelectionRight.Width, SelectionRight.Height), Color.White,
                                 0, Vector2.Zero, SpriteEffects.None, 0);

                //Draws selection box over selected tool
                spriteBatch.Draw(SelectionLeft,
                                 new Rectangle((int)(selectionLeft_Pos.X), (int)(selectionLeft_Pos.Y),
                                               SelectionLeft.Width, SelectionLeft.Height),
                                 new Rectangle(0, 0, SelectionLeft.Width, SelectionLeft.Height), Color.White,
                                 0, Vector2.Zero, SpriteEffects.None, 0);
            }
            // Draws currently selected tools to screen.
            //textDrawingService.DrawTextToScreenLeft(spriteBatch, 0,
            //                                      "Tools Selected: Left(" + currentTool + ") Right(" + currentRightTool +
            //                                      ")");
        }

        public virtual void Update(ref int tool, ref int rightTool)
        {
            if (hud_Up)
            {
                isInHud = CurrentMouseSpot(MouseManager.CurrentPosition);
                    // Checks if mouse is in Selection Box and sets current tool.
            }

            if (MouseManager.CurrentPosition.Y > (gameHeight - SelectionBox.Height/6))
            {
                hud_Moving = true;
                hud_Down = false;
            }

            if (hud_Moving)
            {
                hud_Pos.Y -= 10;
            }
            if (hud_Down)
            {
                hud_Pos.Y += 15;
            }
            if (hud_Pos.Y > gameHeight)
            {
                hud_Pos.Y = gameHeight;
            }

            if (MouseManager.CurrentPosition.Y < (gameHeight - SelectionBox.Height))
            {
                hud_Moving = false;
                hud_Up = false;
                hud_Down = true;
            }


            // Iterates current tool up.
            if (KeyboardManager.currentState.IsKeyDown(Keys.I)
                && KeyboardManager.oldState.IsKeyUp(Keys.I))
                currentTool++;
            // Iterates current tool down.
            if (KeyboardManager.currentState.IsKeyDown(Keys.K)
                && KeyboardManager.oldState.IsKeyUp(Keys.K))
                currentTool--;
        }

        /// <summary>
        /// Checks if mouse is clicked, then determines if it's in the Selection Panel.
        /// </summary>
        /// <param name="currentMousePosition"></param>
        /// <returns></returns>
        public Boolean CurrentMouseSpot(Vector2 currentMousePosition)
        {
            if (MouseManager.LeftButtonPressed)
            {
                #region Tool Selection Mouse Math

                if (currentMousePosition.X > ((gameWidth/2) - (SelectionBox.Width/2)) // Slot 1
                    && currentMousePosition.Y > (gameHeight - SelectionBox.Height)
                    && currentMousePosition.X < (gameWidth/2) - (SelectionBox.Width/2) + cellSizeX
                    && currentMousePosition.Y < (gameHeight - SelectionBox.Height) + cellSizeY)
                {
                    selectionLeft_Pos = new Vector2(((gameWidth/2) - (SelectionBox.Width/2)),
                                                    (gameHeight - SelectionBox.Height));
                    currentTool = -1;
                    return true;
                }
                if (currentMousePosition.X > ((gameWidth/2) - (SelectionBox.Width/2)) // Slot 1
                    && currentMousePosition.Y > (gameHeight - SelectionBox.Height) + cellSizeY
                    && currentMousePosition.X < (gameWidth/2) - (SelectionBox.Width/2) + cellSizeX
                    && currentMousePosition.Y < (gameHeight))
                {
                    selectionLeft_Pos = new Vector2(((gameWidth/2) - (SelectionBox.Width/2)),
                                                    (gameHeight - SelectionBox.Height) + cellSizeY);
                    currentTool = 2;
                    return true;
                }

                else if (currentMousePosition.X > ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX)
                         && currentMousePosition.Y > (gameHeight - SelectionBox.Height) + cellSizeY
                         && currentMousePosition.X < ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*2)
                         && currentMousePosition.Y < (gameHeight))
                {
                    selectionLeft_Pos = new Vector2(((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX),
                                                    (gameHeight - SelectionBox.Height) + cellSizeY);
                    currentTool = 17;
                    return true;
                }
                else if (currentMousePosition.X > ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*2)
                         && currentMousePosition.Y > (gameHeight - SelectionBox.Height)
                         && currentMousePosition.X < ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*3)
                         && currentMousePosition.Y < (gameHeight - SelectionBox.Height) + cellSizeY)
                {
                    selectionLeft_Pos = new Vector2(((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*2),
                                                    (gameHeight - SelectionBox.Height));
                    currentTool = 0;
                    return true;
                }
                else if (currentMousePosition.X > ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX)
                         && currentMousePosition.Y > (gameHeight - SelectionBox.Height)
                         && currentMousePosition.X < ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*2)
                         && currentMousePosition.Y < (gameHeight - SelectionBox.Height) + cellSizeY)
                {
                    selectionLeft_Pos = new Vector2(((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX),
                                                    (gameHeight - SelectionBox.Height));
                    currentTool = 1;
                    return true;
                }
                else if (currentMousePosition.X > ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*3)
                         && currentMousePosition.Y > (gameHeight - SelectionBox.Height)
                         && currentMousePosition.X < ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*4)
                         && currentMousePosition.Y < (gameHeight - SelectionBox.Height) + cellSizeY)
                {
                    selectionLeft_Pos = new Vector2(((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*3),
                                                    (gameHeight - SelectionBox.Height));
                    currentTool = 7;
                    return true;
                }
                else if (currentMousePosition.X > ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*4)
                         && currentMousePosition.Y > (gameHeight - SelectionBox.Height)
                         && currentMousePosition.X < ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*5)
                         && currentMousePosition.Y < (gameHeight - SelectionBox.Height) + cellSizeY)
                {
                    selectionLeft_Pos = new Vector2(((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*4),
                                                    (gameHeight - SelectionBox.Height));
                    currentTool = 8;
                    return true;
                }
                else if (currentMousePosition.X > ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*5)
                         && currentMousePosition.Y > (gameHeight - SelectionBox.Height)
                         && currentMousePosition.X < ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*6)
                         && currentMousePosition.Y < (gameHeight - SelectionBox.Height) + cellSizeY)
                {
                    selectionLeft_Pos = new Vector2(((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*5),
                                                    (gameHeight - SelectionBox.Height));
                    currentTool = 9;
                    return true;
                }
                else if (currentMousePosition.X > ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*6)
                         && currentMousePosition.Y > (gameHeight - SelectionBox.Height)
                         && currentMousePosition.X < ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*7)
                         && currentMousePosition.Y < (gameHeight - SelectionBox.Height) + cellSizeY)
                {
                    selectionLeft_Pos = new Vector2(((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*6),
                                                    (gameHeight - SelectionBox.Height));
                    currentTool = 10;
                    return true;
                }
                else if (currentMousePosition.X > ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*7)
                         && currentMousePosition.Y > (gameHeight - SelectionBox.Height)
                         && currentMousePosition.X < ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*8)
                         && currentMousePosition.Y < (gameHeight - SelectionBox.Height) + cellSizeY)
                {
                    selectionLeft_Pos = new Vector2(((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*7),
                                                    (gameHeight - SelectionBox.Height));
                    currentTool = 4;
                    return true;
                }
                else if (currentMousePosition.X > ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*8)
                         && currentMousePosition.Y > (gameHeight - SelectionBox.Height)
                         && currentMousePosition.X < ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*9)
                         && currentMousePosition.Y < (gameHeight - SelectionBox.Height) + cellSizeY)
                {
                    selectionLeft_Pos = new Vector2(((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*8),
                                                    (gameHeight - SelectionBox.Height));
                    currentTool = 3;
                    return true;
                }
                else if (currentMousePosition.X > ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*9)
                         && currentMousePosition.Y > (gameHeight - SelectionBox.Height)
                         && currentMousePosition.X < ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*10)
                         && currentMousePosition.Y < (gameHeight - SelectionBox.Height) + cellSizeY)
                {
                    selectionLeft_Pos = new Vector2(((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*9),
                                                    (gameHeight - SelectionBox.Height));
                    currentTool = 6;
                    return true;
                }
                else if (currentMousePosition.X > ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*10)
                         && currentMousePosition.Y > (gameHeight - SelectionBox.Height)
                         && currentMousePosition.X < ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*11)
                         && currentMousePosition.Y < (gameHeight - SelectionBox.Height) + cellSizeY)
                {
                    selectionLeft_Pos = new Vector2(((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*10),
                                                    (gameHeight - SelectionBox.Height));
                    currentTool = 5;
                    return true;
                }
                else if (currentMousePosition.X > ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*11)
                         && currentMousePosition.Y > (gameHeight - SelectionBox.Height)
                         && currentMousePosition.X < ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*12)
                         && currentMousePosition.Y < (gameHeight - SelectionBox.Height) + cellSizeY)
                {
                    selectionLeft_Pos = new Vector2(((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*11),
                                                    (gameHeight - SelectionBox.Height));
                    currentTool = 11;
                    return true;
                }
                else if (currentMousePosition.X > ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*12)
                         && currentMousePosition.Y > (gameHeight - SelectionBox.Height)
                         && currentMousePosition.X < ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*13)
                         && currentMousePosition.Y < (gameHeight - SelectionBox.Height) + cellSizeY)
                {
                    selectionLeft_Pos = new Vector2(((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*12),
                                                    (gameHeight - SelectionBox.Height));
                    currentTool = 12;
                    return true;
                }
                else if (currentMousePosition.X > ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*13)
                         && currentMousePosition.Y > (gameHeight - SelectionBox.Height)
                         && currentMousePosition.X < ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*14)
                         && currentMousePosition.Y < (gameHeight - SelectionBox.Height) + cellSizeY)
                {
                    selectionLeft_Pos = new Vector2(((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*13),
                                                    (gameHeight - SelectionBox.Height));
                    currentTool = 15;
                    return true;
                }
                else if (currentMousePosition.X > ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*14)
                         && currentMousePosition.Y > (gameHeight - SelectionBox.Height)
                         && currentMousePosition.X < ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*15)
                         && currentMousePosition.Y < (gameHeight - SelectionBox.Height) + cellSizeY)
                {
                    selectionLeft_Pos = new Vector2(((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*14),
                                                    (gameHeight - SelectionBox.Height));
                    currentTool = 16;
                    return true;
                }
                else if (currentMousePosition.X > ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*15)
                         && currentMousePosition.Y > (gameHeight - SelectionBox.Height)
                         && currentMousePosition.X < ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*16)
                         && currentMousePosition.Y < (gameHeight - SelectionBox.Height) + cellSizeY)
                {
                    selectionLeft_Pos = new Vector2(((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*15),
                                                    (gameHeight - SelectionBox.Height));
                    currentTool = 13;
                    return true;
                }
                else if (currentMousePosition.X > ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*16)
                         && currentMousePosition.Y > (gameHeight - SelectionBox.Height)
                         && currentMousePosition.X < ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*17)
                         && currentMousePosition.Y < (gameHeight - SelectionBox.Height) + cellSizeY)
                {
                    selectionLeft_Pos = new Vector2(((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*16),
                                                    (gameHeight - SelectionBox.Height));
                    currentTool = 14;
                    return true;
                }

                #endregion

                return false;
            }

            if (MouseManager.RightButtonPressed)
            {
                #region Right Tool Selection Mouse Math

                if (currentMousePosition.X > ((gameWidth/2) - (SelectionBox.Width/2)) // Slot 1
                    && currentMousePosition.Y > (gameHeight - SelectionBox.Height)
                    && currentMousePosition.X < (gameWidth/2) - (SelectionBox.Width/2) + cellSizeX
                    && currentMousePosition.Y < (gameHeight - SelectionBox.Height) + cellSizeY)
                {
                    selectionRight_Pos = new Vector2(((gameWidth/2) - (SelectionBox.Width/2)),
                                                     (gameHeight - SelectionBox.Height));
                    currentRightTool = -1;
                    return true;
                }
                if (currentMousePosition.X > ((gameWidth/2) - (SelectionBox.Width/2)) // Slot 1
                    && currentMousePosition.Y > (gameHeight - SelectionBox.Height) + cellSizeY
                    && currentMousePosition.X < (gameWidth/2) - (SelectionBox.Width/2) + cellSizeX
                    && currentMousePosition.Y < (gameHeight))
                {
                    selectionRight_Pos = new Vector2(((gameWidth/2) - (SelectionBox.Width/2)),
                                                     (gameHeight - SelectionBox.Height) + cellSizeY);
                    currentRightTool = 2;
                    return true;
                }

                else if (currentMousePosition.X > ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX)
                         && currentMousePosition.Y > (gameHeight - SelectionBox.Height) + cellSizeY
                         && currentMousePosition.X < ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*2)
                         && currentMousePosition.Y < (gameHeight))
                {
                    selectionRight_Pos = new Vector2(((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX),
                                                     (gameHeight - SelectionBox.Height) + cellSizeY);
                    currentRightTool = 17;
                    return true;
                }
                else if (currentMousePosition.X > ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*2)
                         && currentMousePosition.Y > (gameHeight - SelectionBox.Height)
                         && currentMousePosition.X < ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*3)
                         && currentMousePosition.Y < (gameHeight - SelectionBox.Height) + cellSizeY)
                {
                    selectionRight_Pos = new Vector2(((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*2),
                                                     (gameHeight - SelectionBox.Height));
                    currentRightTool = 0;
                    return true;
                }
                else if (currentMousePosition.X > ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX)
                         && currentMousePosition.Y > (gameHeight - SelectionBox.Height)
                         && currentMousePosition.X < ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*2)
                         && currentMousePosition.Y < (gameHeight - SelectionBox.Height) + cellSizeY)
                {
                    selectionRight_Pos = new Vector2(((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX),
                                                     (gameHeight - SelectionBox.Height));
                    currentRightTool = 1;
                    return true;
                }
                else if (currentMousePosition.X > ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*3)
                         && currentMousePosition.Y > (gameHeight - SelectionBox.Height)
                         && currentMousePosition.X < ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*4)
                         && currentMousePosition.Y < (gameHeight - SelectionBox.Height) + cellSizeY)
                {
                    selectionRight_Pos = new Vector2(((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*3),
                                                     (gameHeight - SelectionBox.Height));
                    currentRightTool = 7;
                    return true;
                }
                else if (currentMousePosition.X > ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*4)
                         && currentMousePosition.Y > (gameHeight - SelectionBox.Height)
                         && currentMousePosition.X < ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*5)
                         && currentMousePosition.Y < (gameHeight - SelectionBox.Height) + cellSizeY)
                {
                    selectionRight_Pos = new Vector2(((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*4),
                                                     (gameHeight - SelectionBox.Height));
                    currentRightTool = 8;
                    return true;
                }
                else if (currentMousePosition.X > ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*5)
                         && currentMousePosition.Y > (gameHeight - SelectionBox.Height)
                         && currentMousePosition.X < ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*6)
                         && currentMousePosition.Y < (gameHeight - SelectionBox.Height) + cellSizeY)
                {
                    selectionRight_Pos = new Vector2(((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*5),
                                                     (gameHeight - SelectionBox.Height));
                    currentRightTool = 9;
                    return true;
                }
                else if (currentMousePosition.X > ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*6)
                         && currentMousePosition.Y > (gameHeight - SelectionBox.Height)
                         && currentMousePosition.X < ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*7)
                         && currentMousePosition.Y < (gameHeight - SelectionBox.Height) + cellSizeY)
                {
                    selectionRight_Pos = new Vector2(((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*6),
                                                     (gameHeight - SelectionBox.Height));
                    currentRightTool = 10;
                    return true;
                }
                else if (currentMousePosition.X > ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*7)
                         && currentMousePosition.Y > (gameHeight - SelectionBox.Height)
                         && currentMousePosition.X < ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*8)
                         && currentMousePosition.Y < (gameHeight - SelectionBox.Height) + cellSizeY)
                {
                    selectionRight_Pos = new Vector2(((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*7),
                                                     (gameHeight - SelectionBox.Height));
                    currentRightTool = 4;
                    return true;
                }
                else if (currentMousePosition.X > ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*8)
                         && currentMousePosition.Y > (gameHeight - SelectionBox.Height)
                         && currentMousePosition.X < ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*9)
                         && currentMousePosition.Y < (gameHeight - SelectionBox.Height) + cellSizeY)
                {
                    selectionRight_Pos = new Vector2(((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*8),
                                                     (gameHeight - SelectionBox.Height));
                    currentRightTool = 3;
                    return true;
                }
                else if (currentMousePosition.X > ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*9)
                         && currentMousePosition.Y > (gameHeight - SelectionBox.Height)
                         && currentMousePosition.X < ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*10)
                         && currentMousePosition.Y < (gameHeight - SelectionBox.Height) + cellSizeY)
                {
                    selectionRight_Pos = new Vector2(((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*9),
                                                     (gameHeight - SelectionBox.Height));
                    currentRightTool = 6;
                    return true;
                }
                else if (currentMousePosition.X > ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*10)
                         && currentMousePosition.Y > (gameHeight - SelectionBox.Height)
                         && currentMousePosition.X < ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*11)
                         && currentMousePosition.Y < (gameHeight - SelectionBox.Height) + cellSizeY)
                {
                    selectionRight_Pos = new Vector2(((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*10),
                                                     (gameHeight - SelectionBox.Height));
                    currentRightTool = 5;
                    return true;
                }
                else if (currentMousePosition.X > ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*11)
                         && currentMousePosition.Y > (gameHeight - SelectionBox.Height)
                         && currentMousePosition.X < ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*12)
                         && currentMousePosition.Y < (gameHeight - SelectionBox.Height) + cellSizeY)
                {
                    selectionRight_Pos = new Vector2(((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*11),
                                                     (gameHeight - SelectionBox.Height));
                    currentRightTool = 11;
                    return true;
                }
                else if (currentMousePosition.X > ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*12)
                         && currentMousePosition.Y > (gameHeight - SelectionBox.Height)
                         && currentMousePosition.X < ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*13)
                         && currentMousePosition.Y < (gameHeight - SelectionBox.Height) + cellSizeY)
                {
                    selectionRight_Pos = new Vector2(((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*12),
                                                     (gameHeight - SelectionBox.Height));
                    currentRightTool = 12;
                    return true;
                }
                else if (currentMousePosition.X > ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*13)
                         && currentMousePosition.Y > (gameHeight - SelectionBox.Height)
                         && currentMousePosition.X < ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*14)
                         && currentMousePosition.Y < (gameHeight - SelectionBox.Height) + cellSizeY)
                {
                    selectionRight_Pos = new Vector2(((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*13),
                                                     (gameHeight - SelectionBox.Height));
                    currentRightTool = 15;
                    return true;
                }
                else if (currentMousePosition.X > ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*14)
                         && currentMousePosition.Y > (gameHeight - SelectionBox.Height)
                         && currentMousePosition.X < ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*15)
                         && currentMousePosition.Y < (gameHeight - SelectionBox.Height) + cellSizeY)
                {
                    selectionRight_Pos = new Vector2(((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*14),
                                                     (gameHeight - SelectionBox.Height));
                    currentRightTool = 16;
                    return true;
                }
                else if (currentMousePosition.X > ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*15)
                         && currentMousePosition.Y > (gameHeight - SelectionBox.Height)
                         && currentMousePosition.X < ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*16)
                         && currentMousePosition.Y < (gameHeight - SelectionBox.Height) + cellSizeY)
                {
                    selectionRight_Pos = new Vector2(((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*15),
                                                     (gameHeight - SelectionBox.Height));
                    currentRightTool = 13;
                    return true;
                }
                else if (currentMousePosition.X > ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*16)
                         && currentMousePosition.Y > (gameHeight - SelectionBox.Height)
                         && currentMousePosition.X < ((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*17)
                         && currentMousePosition.Y < (gameHeight - SelectionBox.Height) + cellSizeY)
                {
                    selectionRight_Pos = new Vector2(((gameWidth/2) - (SelectionBox.Width/2)) + (cellSizeX*16),
                                                     (gameHeight - SelectionBox.Height));
                    currentRightTool = 14;
                    return true;
                }

                #endregion
            }
            return false;
        }

        public bool isHudUp()
        {
            return hud_Up;
        }
    }
}