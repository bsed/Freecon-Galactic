using Freecon.Client.Core.Interfaces;
using Microsoft.Xna.Framework;
using Core.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Concurrent;

namespace Freecon.Client.GUI
{
    public class TextDrawingService : ISynchronousUpdate
    {
        // Fonts for rendering
        public static SpriteFont DefaultDrawFont;

        // Variables for Drawspaces
        private int leftMargin = 5, rightMargin = 5, topMargin = 5;
        private int slotSize = 14;

        // Performance & Update Variables

        private int fps,
                    fpsIteration;

        private int totalIterations = 0;
        private int iterationsPerSecond;
        private float oneSecondTimer;
        private int ups;


        
        ConcurrentBag<DrawString> _stringsToDraw;

        SpriteBatch _spriteBatch;

        /// <summary>
        /// Contructor for initializing new instance of class.
        /// </summary>
        /// <param name="Content">Pass Content from Main.</param>
        public TextDrawingService(SpriteFont defaultDrawFont, SpriteBatch spriteBatch)
        {
            DefaultDrawFont = defaultDrawFont;

            Freecon.Client.Managers.Debugging.textDrawingService = this;
            _spriteBatch = spriteBatch;

            _stringsToDraw = new ConcurrentBag<DrawString>();

        }

        public virtual void Update(IGameTimeService gameTime)
        {
            // Performance & Time Variables
            oneSecondTimer += gameTime.ElapsedMilliseconds; // Timer variable
            totalIterations++; // Amount of times Update has been called

            if (oneSecondTimer > 1000) // If 1000ms has passed, set performance variables and reset.
            {
                ups = iterationsPerSecond; // Updates per second
                fps = fpsIteration;
                iterationsPerSecond = totalIterations;
                totalIterations = 0;
                oneSecondTimer = 0;
                fpsIteration = 0;
            }
        }


        public void Draw()
        {
            _spriteBatch.Begin();

            foreach(DrawString ds in _stringsToDraw)
            {
                _spriteBatch.DrawString(ds.DrawFont, ds.Text, ds.DrawPos, ds.DrawColor, ds.DrawRotation, ds.DrawOrigin, ds.DrawScale, SpriteEffects.None, 0);
            }
            _spriteBatch.End();

            _stringsToDraw = new ConcurrentBag<DrawString>();//"Clears" the bag. Might cause strings not to draw during concurrency, in which case some blocking needs to be implemented.

        }

        /// <summary>
        /// Draws text to exact position, origin at top left.
        /// </summary>
        /// <param name="spriteBatch">Passed spritebatch.</param>
        /// <param name="position">Position to draw from.</param>
        /// <param name="message">String to be drawn.</param>
        public void DrawTextToScreen(Vector2 position, string message)
        {
            DrawString ds = new DrawString(message, position.X, position.Y);
            _stringsToDraw.Add(ds);
            
        }

        /// <summary>
        /// Draws to left of screen in a predetermined space.
        /// </summary>
        /// <param name="spriteBatch">Passed spritebatch.</param>
        /// <param name="slot">Slot number to draw in, from top to bottom.</param>
        /// <param name="message">String to be drawn.</param>
        public void DrawTextToScreenLeft(int slot, string message)        {

            Vector2 drawPos = new Vector2(leftMargin, (slot*slotSize) + topMargin);
            DrawString ds = new DrawString(message, drawPos.X, drawPos.Y);
            _stringsToDraw.Add(ds);
        }

        /// <summary>
        /// Draws to right of screen in a predetermined space.
        /// </summary>
        /// <param name="spriteBatch">Passed spritebatch.</param>
        /// <param name="slot">Slot number to draw in, from top to bottom.</param>
        /// <param name="message">String to be drawn.</param>
        public void DrawTextToScreenRight(int slot, string message)
        {
            Vector2 stringSize = DefaultDrawFont.MeasureString(message);
            Vector2 drawPos = new Vector2(_spriteBatch.GraphicsDevice.Viewport.Width - rightMargin - stringSize.X, (slot * slotSize) + topMargin);// Specifies to draw within viewport
            DrawString ds = new DrawString(message, drawPos.X, drawPos.Y);
            _stringsToDraw.Add(ds);            
        }
        
        /// <summary>
        /// Draws text at specified location.
        /// </summary>
        /// <param name="spriteBatch">Passed spritebatch.</param>
        /// <param name="location">Location to draw to.</param>
        /// <param name="message">String to be drawn.</param>
        public void DrawTextAtLocation(Vector2 location, string message, float scale = 1, Color? color = null)
        {
            DrawString ds = new DrawString(message, location.X, location.Y);
            if (color != null)
            {
                ds.DrawColor = (Color)color;
            }
            ds.DrawScale = scale;
            _stringsToDraw.Add(ds);            
        }

        /// <summary>
        /// Draws text at specified location.
        /// </summary>
        /// <param name="spriteBatch">Passed spritebatch.</param>
        /// <param name="location">Location to draw to.</param>
        /// <param name="message">String to be drawn.</param>
        /// <param name="message">Zoom level.</param>
        public void DrawTextAtLocation(SpriteBatch spriteBatch, Vector2 location, string message, float scale, Color? color = null)
        {
            DrawString ds = new DrawString(message, location.X, location.Y);
            if (color != null)
            {
                ds.DrawColor = (Color)color;
            }

            ds.DrawScale = scale;
            _stringsToDraw.Add(ds);       
        }

        
        /// <summary>
        /// Draws text at specified location, with the text's middle at the location.
        /// </summary>
        /// <param name="spriteBatch">Passed spritebatch.</param>
        /// <param name="location">Location to draw to.</param>
        /// <param name="message">String to be drawn.</param>
        public void DrawTextAtLocationCentered(Vector2 location, string message, float scale = 1, Color? color = null)
        {
            Vector2 stringSize = DefaultDrawFont.MeasureString(message) * scale;
            DrawTextAtLocation(new Vector2(location.X - stringSize.X/2, location.Y - stringSize.Y/2),
                               message, scale, color);
        }

        /// <summary>
        /// Draws text at specified location, with the text's middle at the location.
        /// </summary>
        /// <param name="spriteBatch">Passed spritebatch.</param>
        /// <param name="location">Location to draw to.</param>
        /// <param name="message">String to be drawn.</param>
        public void DrawTextAtLocationCentered(SpriteBatch spriteBatch, Vector2 location, string message, float scale, Color? color = null)
        {
            Vector2 stringSize = DefaultDrawFont.MeasureString(message);
            DrawTextAtLocation(spriteBatch,
                               new Vector2(location.X - stringSize.X/scale/2, location.Y - stringSize.Y/scale/2),
                               message, scale, color);
        }

       
        public SpriteFont GetFont()
        {
            return DefaultDrawFont;
        }

        public int getFPS()
        {
            return iterationsPerSecond;

        }

        
    }

    class DrawString
    {
        public SpriteFont DrawFont = TextDrawingService.DefaultDrawFont;
        public string Text;
        public Vector2 DrawPos;
        public Color DrawColor = Color.Aquamarine;
        public float DrawScale = 1;
        public SpriteEffects SpriteEffects = SpriteEffects.None;
        public float DrawRotation = 0;
        public Vector2 DrawOrigin = new Vector2(0, 0);


        public DrawString(string text, float xPos, float yPos)
        {
            Text = text;
            DrawPos = new Vector2(xPos, yPos);
        }

        


    }
}